import * as React from 'react';
import { Data , Facility} from './typings/data';
import { toUInt32, toInt32, toInt16, valueAsInt32IsNegative, valueAsInt16IsNegative, hex4, hex8 } from './helpers';
import Simple16BitCode from './simple-16bit-code';

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

function AnalyzeNtStatus({ data, code }: { data: Data, code: number }) {
    const ntstatusCode = new NtStatusCode(data, code);

    return (
        <div>
            <div>{ severityNames[ntstatusCode.severity] } Code</div>
            { ntstatusCode.ntstatus ? <div>The reserved bit N is set.</div> : null }
            { ntstatusCode.customer ? <div>This is a third-party error code, not a Microsoft error code.</div> : null }
            { ntstatusCode.facilityCodeValid ? <div>Facility: <Simple16BitCode code={ntstatusCode.facilityCode} /></div> : null }
            { ntstatusCode.facility ? <div>{ntstatusCode.facility.names.map(x => <div key={x.name}>{x.name}</div>) }</div> : null }
            { ntstatusCode.errorCodeValid ? <div>Code: <Simple16BitCode code={ntstatusCode.errorCode} /></div> : null }
        </div>
    );
}

export default AnalyzeNtStatus;