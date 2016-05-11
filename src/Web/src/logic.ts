import * as _ from 'lodash';

export type QueryType = 'win32' | 'hresult' | 'ntstatus';

const validHexNumber = /^(0x)?[0-9A-F]{1,8}$/i;
const validDecNumber = /^-?[0-9]+$/;

type LogicCallback = (errorMessage: ErrorMessage) => void;

export function handleWin32Code(callback: LogicCallback, data: Data, code: number) {
    const err = _.find(data.win32, x => x.code === code);
    if (err) {
        callback(err);
    }
}

export function ntStatusUnwrapWin32(code: number): number {
    if ((code & 0xFFFF0000) === 0xC0070000) {
        return code & 0xFFFF;
    }
    return NaN;
}

export function handleNtStatusCode(callback: LogicCallback, data: Data, code: number) {
    // Do a direct NTSTATUS lookup first.
    const err = _.find(data.ntStatus, x => x.code === code);
    if (err) {
        callback(err);
    }

    // If this is an NTSTATUS wrapper around a Win32 error, include the Win32 error as well.
    const win32Code = ntStatusUnwrapWin32(code);
    if (!isNaN(win32Code)) {
        handleWin32Code(callback, data, win32Code);
    }
}

export function hresultUnwrapWin32(code: number): number {
    if ((code & 0xFFFF0000) === 0x80070000) {
        return code & 0xFFFF;
    }
    return NaN;
}

export function hresultUnwrapNtStatus(code: number): number {
    if ((code & 0x10000000) === 0x10000000) {
        return code & 0xEFFFFFFF;
    }
    return NaN;
}

export function hresultUnwrapFilterManagerNtStatus(code: number): number {
    if ((code & 0x1FFF0000) === 0x001F000) {
        return (code & 0x8000FFFF) | 0x401C0000;
    }
    return NaN;
}

export function handleHresultCode(callback: LogicCallback, data: Data, code: number) {
    // Do a direct HRESULT lookup first.
    const err = _.find(data.hresult, x => x.code === code);
    if (err) {
        callback(err);
    }

    // If this is an HRESULT wrapper around a Win32 error, include the Win32 error as well.
    const win32Code = hresultUnwrapWin32(code);
    if (!isNaN(win32Code)) {
        handleWin32Code(callback, data, win32Code);
    }

    // If this is an HRESULT wrapper around an NTSTATUS, include the NTSTATUS error as well (which can also include NTSTATUS wrappers).
    const ntStatusCode = hresultUnwrapNtStatus(code);
    if (!isNaN(ntStatusCode)) {
        handleNtStatusCode(callback, data, ntStatusCode);
    }

    // The filter manager has its own HRESULT-wrapper-around-NTSTATUS system.
    //  See FILTER_FLT_NTSTATUS_FROM_HRESULT (in ntstatus.h) and FILTER_HRESULT_FROM_FLT_NTSTATUS (in winerror.h)
    const filterManagerNtStatusCode = hresultUnwrapFilterManagerNtStatus(code);
    if (!isNaN(filterManagerNtStatusCode)) {
        handleNtStatusCode(callback, data, filterManagerNtStatusCode);
    }
}

/**
 * Attempts to match the user query to one or more error messages. Returns an array of possible matches, which may be empty.
 * @param query The user query.
 * @param type The type of the user query, if known. This may be null, undefined, or an empty string to indicate the type is unknown.
 * @param data The database of known error messages.
 */
export function search(query: string, type: QueryType | void, data: Data): ErrorMessage[] {
    const result: ErrorMessage[] = [];

    const mayBeWin32 = type === 'win32' || !type;
    const mayBeNtStatus = type === 'ntstatus' || !type;
    const mayBeHresult = type === 'hresult' || !type;

    // If this is a valid number, then only evaluate it as a number (matching against code).
    const isValidHexNumber = validHexNumber.test(query);
    const isValidDecNumber = validDecNumber.test(query);
    if (isValidHexNumber || isValidDecNumber) {
        const appendCallback: LogicCallback = err => {
            if (result.indexOf(err) < 0) {
                result.push(err);
            }
        };

        // Attempt to match as a hex number first.
        if (isValidHexNumber) {
            const code = parseInt(query, 16);
            if (mayBeWin32) {
                handleWin32Code(appendCallback, data, code);
            }
            if (mayBeHresult) {
                handleHresultCode(appendCallback, data, code);
            }
            if (mayBeNtStatus) {
                handleNtStatusCode(appendCallback, data, code);
            }
        }

        // Next, attempt to match as a dec number.
        if (isValidDecNumber) {
            const code = parseInt(query, 10);
            if (mayBeWin32) {
                handleWin32Code(appendCallback, data, code);
            }
            if (mayBeHresult) {
                handleHresultCode(appendCallback, data, code);
            }
            if (mayBeNtStatus) {
                handleNtStatusCode(appendCallback, data, code);
            }
        }
    }

    // Perform a case-insensitive substring match on the symbolic id(s) and error text, in that order.
    const regex = new RegExp(_.escapeRegExp(query), 'i');
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