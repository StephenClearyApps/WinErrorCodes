import React from 'react';
import { Data, Facility } from './typings/data';
import { hresultUnwrapNtStatus, hresultUnwrapFilterManagerNtStatus } from './logic';
import { toUInt32 } from './helpers';
import AnalyzeNtStatus from './analyze-ntstatus';
import Simple16BitCode from './simple-16bit-code';

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

function AnalyzeHResult({ data, code }: { data: Data, code: number }) {
    const hresultCode = new HResultCode(data, code);

    return (
        <div>
            <div>{ hresultCode.severity ? 'Error' : 'Success' } Code</div>
            { hresultCode.reserved ? <div>The reserved bit R is set.</div> : null }
            { hresultCode.xreserved ? <div>The reserved bit X is set.</div> : null }
            { hresultCode.customer ? <div>This is a third-party error code, not a Microsoft error code.</div> : null }
            { hresultCode.facilityCodeValid ? <div>Facility: <Simple16BitCode code={hresultCode.facilityCode} /></div> : null }
            { hresultCode.facility ? <div>{hresultCode.facility.names.map(x => <div key={x.name}><code>{x.name}</code></div>) }</div> : null }
            { hresultCode.errorCodeValid ? <div>Code: <Simple16BitCode code={hresultCode.errorCode} /></div> : null }
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