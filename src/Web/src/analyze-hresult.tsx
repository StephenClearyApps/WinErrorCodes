import React from 'react';
import { Data, Facility } from './typings/data';
import { hresultUnwrapNtStatus, hresultUnwrapFilterManagerNtStatus } from './logic';
import { toUInt32, hex8 } from './helpers';
import AnalyzeNtStatus from './analyze-ntstatus';
import Simple16BitCode from './simple-16bit-code';
import Spanner from './spanner';

class HResultCode {
    severity: number;
    reserved: number;
    customer: number;
    ntstatus: number;
    xreserved: number;
    facilityCode: number;
    errorCode: number;

    facility: Facility;
    ntstatusCode: number;
    get facilityCodeValid(): boolean {
        return !this.customer && !this.ntstatus;
    }
    get errorCodeValid(): boolean {
        return !this.customer && !this.ntstatus;
    }

    constructor(data: Data, code: number) {
        this.severity = toUInt32(code & 0x80000000);
        this.reserved = toUInt32(code & 0x40000000);
        this.customer = toUInt32(code & 0x20000000);
        this.ntstatus = toUInt32(code & 0x10000000);
        this.xreserved = toUInt32(code & 0x08000000);
        this.facilityCode = toUInt32(code & 0x07FF0000) >>> 16;
        this.errorCode = toUInt32(code & 0x0000FFFF);
        const ntstatusCode = hresultUnwrapNtStatus(code);
        this.ntstatusCode = isNaN(ntstatusCode) ? hresultUnwrapFilterManagerNtStatus(code) : ntstatusCode;
        if (this.facilityCodeValid) {
            this.facility = data.hresultFacilities.find(x => x.value === this.facilityCode);
        }
    }
}

function reservedMessage(hresultCode: HResultCode) {
    return hresultCode.reserved ?
        <span>The reserved bit <code>R</code> is set.This is highly unusual.</span> :
        <span>The reserved bit <code>R</code> is not set.This is normal.</span>;
}

function xreservedMessage(hresultCode: HResultCode) {
    return hresultCode.xreserved ?
        <span>The reserved bit <code>X</code> is set.This is highly unusual.</span> :
        <span>The reserved bit <code>X</code> is not set.This is normal.</span>;
}

function customerMessage(hresultCode: HResultCode) {
    return hresultCode.customer ?
        <span>The customer bit is set.This is a third-party error code, not a Microsoft error code.</span> :
        <span>The customer bit is not set.This is a Microsoft error code.</span>;
}

function ntstatusMessage(hresultCode: HResultCode) {
    return hresultCode.ntstatus ?
        <span>The NTSTATUS bit is set.This HRESULT code is a wrapper around an NTSTATUS code.</span> :
        <span>The NTSTATUS bit is not set.This is a regular HRESULT code.</span>;
}

function AnalyzeHResult({ data, code }: { data: Data, code: number }) {
    const hresultCode = new HResultCode(data, code);
    const hex = hex8(code);

    return (
        <div>
            <div className='panel panel-default'>
                <div className='panel-heading'>Detailed HRESULT analysis of <code>0x{hex}</code></div>
                <div className='panel-body'>
                    <table className='table'>
                        <thead>
                            <tr><th>Code</th><th>Meaning</th></tr>
                        </thead>
                        <tbody>
                            <tr><td><code><Spanner text={hex} ranges={[{ begin: 0, end: 1 }]}/></code></td><td>Severity: { hresultCode.severity ? 'Error' : 'Success' }</td></tr>
                            <tr><td><code><Spanner text={hex} ranges={[{ begin: 0, end: 1 }]}/></code></td><td>{reservedMessage(hresultCode) }</td></tr>
                            <tr><td><code><Spanner text={hex} ranges={[{ begin: 0, end: 1 }]}/></code></td><td>{customerMessage(hresultCode) }</td></tr>
                            <tr><td><code><Spanner text={hex} ranges={[{ begin: 0, end: 1 }]}/></code></td><td>{ntstatusMessage(hresultCode) }</td></tr>
                            <tr><td><code><Spanner text={hex} ranges={[{ begin: 1, end: 2 }]}/></code></td><td>{xreservedMessage(hresultCode) }</td></tr>
                            {
                                hresultCode.facilityCodeValid ?
                                    <tr><td><code><Spanner text={hex} ranges={[{ begin: 1, end: 4 }]}/></code></td><td><div>
                                        <div>Facility: <Simple16BitCode code={hresultCode.facilityCode} /></div>
                                        { hresultCode.facility ? <div>{hresultCode.facility.names.map(x => <div key={x.name}><code>{x.name}</code></div>) }</div> : null }
                                    </div></td></tr> :
                                    null
                            }
                            {
                                hresultCode.errorCodeValid ?
                                    <tr><td><code><Spanner text={hex} ranges={[{ begin: 4, end: 8 }]}/></code></td><td>Code: <Simple16BitCode code={hresultCode.errorCode} /></td></tr> :
                                    null
                            }
                        </tbody>
                    </table>
                </div>
            </div>
            {
                isNaN(hresultCode.ntstatusCode) ? null :
                    <div>
                        <div>This HRESULT code is a wrapper around an NTSTATUS code.</div>
                        <AnalyzeNtStatus data={data} code={hresultCode.ntstatusCode} />
                    </div>
            }
        </div>
    );
}

export default AnalyzeHResult;