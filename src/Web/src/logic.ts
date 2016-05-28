import * as _ from 'lodash';
import { Data, ErrorMessage, ErrorMessageType, errorMessageTypeHumanReadableString } from './typings/data';
import { toUInt32, hex4, hex8 } from './helpers';

export type QueryType = 'win32' | 'hresult' | 'ntstatus';

const validHexNumber = /^(0x)?[0-9A-F]{1,8}$/i;
const validDecNumber = /^-?[0-9]+$/;

/**
 * Matches the code against known Win32 errors, and returns 0-1 matches.
 */
export function appendWin32Codes(result: ErrorMessage[], data: Data, code: number) {
    const err = _.find(data.win32, x => x.code === code);
    if (err && result.indexOf(err) < 0) {
        result.push(err);
    }
}

export function ntStatusUnwrapWin32(code: number): number {
    if (toUInt32(code & 0xFFFF0000) === 0xC0070000) {
        return toUInt32(code & 0xFFFF);
    }
    return NaN;
}

/**
 * Matches the code against known NTSTATUS errors, and returns 0-2 matches.
 */
export function appendNtStatusCodes(result: ErrorMessage[], data: Data, code: number) {
    // Do a direct NTSTATUS lookup first.
    const err = _.find(data.ntStatus, x => x.code === code);
    if (err && result.indexOf(err) < 0) {
        result.push(err);
    }

    // If this is an NTSTATUS wrapper around a Win32 error, include the Win32 error as well.
    const win32Code = ntStatusUnwrapWin32(code);
    if (!isNaN(win32Code)) {
        appendWin32Codes(result, data, win32Code);
    }
}

/**
 * Attempts to unwrap the HRESULT code into a Win32 code. Returns NaN if the HRESULT code is not a wrapper around a Win32 code.
 */
export function hresultUnwrapWin32(code: number): number {
    if (toUInt32(code & 0xFFFF0000) === 0x80070000) {
        return toUInt32(code & 0xFFFF);
    }
    return NaN;
}

/**
 * Attempts to unwrap the HRESULT code into an NTSTATUS code. Returns NaN if the HRESULT code is not a wrapper around an NTSTATUS code.
 */
export function hresultUnwrapNtStatus(code: number): number {
    if (toUInt32(code & 0x10000000) === 0x10000000) {
        return toUInt32(code & 0xEFFFFFFF);
    }
    return NaN;
}

/**
 * Attempts to unwrap the HRESULT code into a filter manager NTSTATUS code. Returns NaN if the HRESULT code is not a wrapper around a filter manager NTSTATUS code.
 */
export function hresultUnwrapFilterManagerNtStatus(code: number): number {
    if (toUInt32(code & 0x1FFF0000) === 0x001F000) {
        return toUInt32(toUInt32(code & 0x8000FFFF) | 0x401C0000);
    }
    return NaN;
}

/**
 * Matches the code against known HRESULT values, and returns 0-3 matches.
 */
export function appendHresultCodes(result: ErrorMessage[], data: Data, code: number) {
    // Do a direct HRESULT lookup first.
    const err = _.find(data.hresult, x => x.code === code);
    if (err && result.indexOf(err) < 0) {
        result.push(err);
    }

    // If this is an HRESULT wrapper around a Win32 error, include the Win32 error as well.
    const win32Code = hresultUnwrapWin32(code);
    if (!isNaN(win32Code)) {
        appendWin32Codes(result, data, win32Code);
    }

    // If this is an HRESULT wrapper around an NTSTATUS, include the NTSTATUS error as well (which can also include NTSTATUS wrappers).
    const ntStatusCode = hresultUnwrapNtStatus(code);
    if (!isNaN(ntStatusCode)) {
        appendNtStatusCodes(result, data, ntStatusCode);
    }

    // The filter manager has its own HRESULT-wrapper-around-NTSTATUS system.
    //  See FILTER_FLT_NTSTATUS_FROM_HRESULT (in ntstatus.h) and FILTER_HRESULT_FROM_FLT_NTSTATUS (in winerror.h)
    const filterManagerNtStatusCode = hresultUnwrapFilterManagerNtStatus(code);
    if (!isNaN(filterManagerNtStatusCode)) {
        appendNtStatusCodes(result, data, filterManagerNtStatusCode);
    }
}

export function findCodes(data: Data, type: QueryType, code: number): ErrorMessage[] {
    const result: ErrorMessage[] = [];
    if (type === 'win32') {
        appendWin32Codes(result, data, code);
    } else if (type === 'ntstatus') {
        appendNtStatusCodes(result, data, code);
    } else if (type === 'hresult') {
        appendHresultCodes(result, data, code);
    }
    return result;
}

/**
 * Returns a unique identifier for a code. This method does all unwrapping necessary.
 */
export function uniqueIdentifier(type: QueryType, code: number): string {
    if (type === 'win32') {
        return 'w' + hex4(code);
    } else if (type === 'hresult') {
        const win32 = hresultUnwrapWin32(code);
        if (!isNaN(win32)) {
            return uniqueIdentifier('win32', win32);
        }
        const ntstatus = hresultUnwrapNtStatus(code);
        if (!isNaN(ntstatus)) {
            return uniqueIdentifier('ntstatus', ntstatus);
        }
        const filterManagerNtStatus = hresultUnwrapFilterManagerNtStatus(code);
        if (!isNaN(filterManagerNtStatus)) {
            return uniqueIdentifier('ntstatus', filterManagerNtStatus);
        }
        return 'h' + hex8(code);
    } else if (type === 'ntstatus') {
        const win32 = ntStatusUnwrapWin32(code);
        if (!isNaN(win32)) {
            return uniqueIdentifier('win32', win32);
        }
        return 'n' + hex8(code);
    }
    return hex8(code);
}

/**
 * Attempts to match the user query to one or more error messages. Returns an array of possible matches, which may be empty.
 * @param query The user query.
 * @param type The type of the user query, if known. This may be null, undefined, or an empty string to indicate the type is unknown.
 * @param data The database of known error messages.
 */
export function search(query: string, type: QueryType | void, data: Data): ErrorMessage[] {
    query = query ? query : '';
    const result: ErrorMessage[] = [];

    const mayBeWin32 = type === 'win32' || !type;
    const mayBeNtStatus = type === 'ntstatus' || !type;
    const mayBeHresult = type === 'hresult' || !type;

    // If this is a valid number, then only evaluate it as a number (matching against code).
    const isValidHexNumber = validHexNumber.test(query);
    const isValidDecNumber = validDecNumber.test(query);
    if (isValidHexNumber || isValidDecNumber) {
        // Attempt to match as a hex number first.
        if (isValidHexNumber) {
            const code = parseInt(query, 16);
            if (mayBeWin32) {
                appendWin32Codes(result, data, code);
            }
            if (mayBeHresult) {
                appendHresultCodes(result, data, code);
            }
            if (mayBeNtStatus) {
                appendNtStatusCodes(result, data, code);
            }
        }

        // Next, attempt to match as a dec number.
        if (isValidDecNumber) {
            const code = parseInt(query, 10);
            if (mayBeWin32) {
                appendWin32Codes(result, data, code);
            }
            if (mayBeHresult) {
                appendHresultCodes(result, data, code);
            }
            if (mayBeNtStatus) {
                appendNtStatusCodes(result, data, code);
            }
        }
    }

    if (result.length) {
        return result;
    }

    const regex = new RegExp(_.escapeRegExp(query), 'i');

    // If the query is not long enough or if it matches too-common strings, then return no matches.
    if (query.length < 3 || regex.test('ERROR_') || regex.test('STATUS_')) {
        return [];
    }

    // Perform a case-insensitive substring match on the symbolic id(s) and error text, in that order.
    const errors = type === 'win32' ? data.win32 :
        type === 'hresult' ? data.hresult :
            type === 'ntstatus' ? data.ntStatus :
                data.win32.concat(data.hresult, data.ntStatus);
    for (let err of errors) {
        if (result.indexOf(err) >= 0) {
            continue;
        }
        for (let id of err.identifiers) {
            if (regex.test(id)) {
                result.push(err);
                break;
            }
        }
    }
    for (let err of errors) {
        if (result.indexOf(err) >= 0) {
            continue;
        }
        if (regex.test(err.text)) {
            result.push(err);
        }
    }

    return result;
}

export function errorMessageUrl(errorMessage: ErrorMessage): string {
    return '/?type=' + encodeURIComponent(errorMessageTypeHumanReadableString(errorMessage.type).toLowerCase()) + '&code=' + encodeURIComponent(hex8(errorMessage.code));
}