import * as React from 'react';
import { toInt32, toInt16, valueAsInt32IsNegative, valueAsInt16IsNegative, hex4, hex8 } from './helpers';

function Code({ code }: { code: number }) {
    const errorCode = ((code & 0xFFFF0000) != 0) ? (code & 0xFFFF) : null;
    return (
        <div>
            <div>{ hex8(code) }</div>
            <div>{ code.toString(10) }</div>
            { valueAsInt32IsNegative(code) ? <div>{ toInt32(code).toString(10) }</div> : null }
            { errorCode === null ? null : <div>{ hex4(errorCode) }</div> }
            { errorCode === null ? null : <div>{ errorCode.toString(10) }</div> }
            { errorCode === null || !valueAsInt16IsNegative(errorCode) ? null : <div>{ toInt16(errorCode).toString(10) }</div> }
        </div>
    );
}

export default Code;