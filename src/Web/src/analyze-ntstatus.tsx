import React from 'react';
import { Data , Facility} from './typings/data';
import { toUInt32, toInt32, toInt16, valueAsInt32IsNegative, valueAsInt16IsNegative, hex4, hex8 } from './helpers';
import Simple16BitCode from './simple-16bit-code';
import Spanner from './spanner';

class NtStatusCode {
    severity: number;
    customer: number;
    ntstatus: number;
    facilityCode: number;
    errorCode: number;

    facility: Facility;
    get facilityCodeValid(): boolean {
        return !this.customer;
    }
    get errorCodeValid(): boolean {
        return !this.customer;
    }

    constructor(data: Data, code: number) {
        this.severity = toUInt32(code & 0xC0000000) >>> 30;
        this.customer = toUInt32(code & 0x20000000);
        this.ntstatus = toUInt32(code & 0x10000000);
        this.facilityCode = toUInt32(code & 0x0FFF0000) >>> 16;
        this.errorCode = toUInt32(code & 0x0000FFFF);
        if (this.facilityCodeValid) {
            this.facility = data.hresultFacilities.find(x => x.value === this.facilityCode);
        }
    }
}

const severityNames = [
    'Success', 'Information', 'Warning', 'Error'
];

function customerMessage(hresultCode: NtStatusCode) {
    return hresultCode.customer ?
        <span>The customer bit is set.This is a third-party error code, not a Microsoft error code.</span> :
        <span>The customer bit is not set.This is a Microsoft error code.</span>;
}

function ntstatusMessage(hresultCode: NtStatusCode) {
    return hresultCode.ntstatus ?
        <span>The reserved bit <code>N</code> is set.This is highly unusual.</span> :
        <span>The reserved bit <code>N</code> is not set.This is normal.</span>;
}

function AnalyzeNtStatus({ data, code }: { data: Data, code: number }) {
    const ntstatusCode = new NtStatusCode(data, code);
    const hex = hex8(code);

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
                                        { ntstatusCode.facility ? <div>{ntstatusCode.facility.names.map(x => <div key={x.name}><code>{x.name}</code></div>) }</div> : null }
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
                </div>
            </div>
        </div>
    );
}

export default AnalyzeNtStatus;