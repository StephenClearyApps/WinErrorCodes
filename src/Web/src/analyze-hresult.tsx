import * as React from 'react';
import { Data } from './typings/data';
import { toUInt32, toInt32, toInt16, valueAsInt32IsNegative, valueAsInt16IsNegative, hex4, hex8 } from './helpers';

function AnalyzeHResult({ data, code }: { data: Data, code: number }) {
    const severity = toUInt32(code & 0x80000000);
    const reserved = toUInt32(code & 0x40000000);
    const customer = toUInt32(code & 0x20000000);
    const ntstatus = toUInt32(code & 0x10000000);
    const xreserved = toUInt32(code & 0x08000000);
    const facilityCode = toUInt32(code & 0x07FF0000) >>> 16;
    const errorCode = toUInt32(code & 0x0000FFFF);

    if (ntstatus) {
        return <div>TODO: analyze NTSTATUS</div>;
    }

    const prefix = (
        <div>
            <div>{ severity ? 'Error Code' : 'Success Code' }</div>
            { reserved ? <div>The reserved bit R is set.</div> : null }
            { xreserved ? <div>The reserved bit X is set.</div> : null }
            { customer ? <div>This is a third-party error code, not a Microsoft error code.</div> : null }
        </div>
    );

    if (customer) {
        return prefix;
    }

    const facility = data.hresultFacilities.find(x => x.value === facilityCode);

    return (
        <div>
            { prefix }
            <div>Facility: 0x{ hex4(facilityCode) }</div>
            <div>{ facility ? facility.names.map(x => <div key={x.name}>{x.name}</div>) : null }</div>
            <div>Code: 0x{ hex4(errorCode) }</div>
        </div>
    );
}

export default AnalyzeHResult;