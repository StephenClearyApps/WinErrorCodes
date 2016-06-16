import _ from 'lodash';
import React from 'react';
import { Link } from 'react-router';
import { Data , Facility, ErrorMessageType } from './typings/data';
import { toUInt32, toInt32, toInt16, valueAsInt32IsNegative, valueAsInt16IsNegative, hex4, hex8 } from './helpers';
import { errorMessageUrl, ntStatusUnwrapWin32 } from './logic';
import Simple16BitCode from './simple-16bit-code';
import Spanner from './spanner';

class NtStatusCode {
    /** The NTSTATUS code */
    fullCode: number;

    /** The severity, in the range [0, 4). */
    severity: number;

    /** Whether the customer bit is set. */
    customer: number;

    /** Whether the NTSTATUS bit is set. */
    ntstatus: number;

    /** The facility code, in the range [0x0000, 0x1000). This value is only valid if `facilityCodeValid`. */
    facilityCode: number;

    /** The error code portion, in the range [0x00000, 0x10000). This value is only valid if `errorCodeValid`. */
    errorCode: number;

    /** The facility; undefined if `!facilityCodeValid` or if `facilityCode` is unknown. */
    facility: Facility;

    /** Whether `facilityCode` is valid. */
    get facilityCodeValid(): boolean {
        return !this.customer;
    }

    /** Whether `errorCode` is valid. */
    get errorCodeValid(): boolean {
        return !this.customer;
    }

    /** Whether `facility` has at least one symbolic identifier. */
    get facilityHasNames(): boolean {
        return this.facility && _.some(this.facility.names, x => x.name);
    }

    constructor(data: Data, code: number) {
        this.fullCode = code;
        this.severity = toUInt32(code & 0xC0000000) >>> 30;
        this.customer = toUInt32(code & 0x20000000);
        this.ntstatus = toUInt32(code & 0x10000000);
        this.facilityCode = toUInt32(code & 0x0FFF0000) >>> 16;
        this.errorCode = toUInt32(code & 0x0000FFFF);
        if (this.facilityCodeValid) {
            this.facility = data.ntStatusFacilities.find(x => x.value === this.facilityCode);
        }
    }
}

const severityNames = [
    'Success', 'Information', 'Warning', 'Error'
];

function customerMessage(code: NtStatusCode) {
    return code.customer ?
        <span>The customer bit is set. This is a third-party error code, not a Microsoft error code.</span> :
        <span>The customer bit is not set. This is a Microsoft error code.</span>;
}

function ntstatusMessage(code: NtStatusCode) {
    return code.ntstatus ?
        <span>The reserved bit <code>N</code> is set. This is highly unusual. <Link to={errorMessageUrl({ type: ErrorMessageType.HResult, code: code.fullCode })}>This error code is probably an HRESULT and not an NTSTATUS.</Link></span> :
        <span>The reserved bit <code>N</code> is not set. This is normal.</span>;
}

function AnalyzeNtStatus({ data, code }: { data: Data, code: number }) {
    const ntstatusCode = new NtStatusCode(data, code);
    const hex = hex8(code);
    const win32Code = ntStatusUnwrapWin32(code);

    return (
        <div>
            <div className='panel panel-default'>
                <div className='panel-heading'>Detailed NTSTATUS analysis of <code>0x{hex}</code></div>
                <div className='panel-body'>
                    <table className='table'>
                        <thead>
                            <tr><th>Code</th><th>Meaning</th></tr>
                        </thead>
                        <tbody>
                            <tr><td><code><Spanner text={hex} ranges={[{ begin: 0, end: 1 }]}/></code></td><td>Severity: { severityNames[ntstatusCode.severity] }</td></tr>
                            <tr><td><code><Spanner text={hex} ranges={[{ begin: 0, end: 1 }]}/></code></td><td>{customerMessage(ntstatusCode) }</td></tr>
                            <tr><td><code><Spanner text={hex} ranges={[{ begin: 0, end: 1 }]}/></code></td><td>{ntstatusMessage(ntstatusCode) }</td></tr>
                            {
                                ntstatusCode.facilityCodeValid ?
                                    <tr><td><code><Spanner text={hex} ranges={[{ begin: 1, end: 4 }]}/></code></td><td><div>
                                        <div>Facility: <Simple16BitCode code={ntstatusCode.facilityCode} /></div>
                                        { ntstatusCode.facilityHasNames ? <div>{ntstatusCode.facility.names.map(x => <div key={x.name}><code>{x.name}</code></div>) }</div> : null }
                                    </div></td></tr> :
                                    null
                            }
                            {
                                ntstatusCode.errorCodeValid ?
                                    <tr><td><code><Spanner text={hex} ranges={[{ begin: 4, end: 8 }]}/></code></td><td>Code: <Simple16BitCode code={ntstatusCode.errorCode} /></td></tr> :
                                    null
                            }
                        </tbody>
                    </table>

                    {
                        isNaN(win32Code) ? null :
                            <p>This NTSTATUS code <code>0x{hex}</code> is a wrapper around <Link to={errorMessageUrl({ type: ErrorMessageType.Win32, code: win32Code})}>the Win32 code <code>{win32Code}</code></Link>.</p>
                    }
                </div>
            </div>
        </div>
    );
}

export default AnalyzeNtStatus;