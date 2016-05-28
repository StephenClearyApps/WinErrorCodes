import * as React from 'react';
import { toInt32, toInt16, valueAsInt32IsNegative, valueAsInt16IsNegative, hex4, hex8 } from './helpers';

function Code({ code }: { code: number }) {
    const errorCode = ((code & 0xFFFF0000) != 0) ? (code & 0xFFFF) : null;
    return (
        <div>
            <div><code>0x{ hex8(code) }</code></div>
            <div><code>{ code.toString(10) }</code></div>
            { valueAsInt32IsNegative(code) ? <div><code>{ toInt32(code).toString(10) }</code></div> : null }
            { errorCode === null ? null : <div><code>0x{ hex4(errorCode) }</code></div> }
            { errorCode === null ? null : <div><code>{ errorCode.toString(10) }</code></div> }
            { errorCode === null || !valueAsInt16IsNegative(errorCode) ? null : <div><code>{ toInt16(errorCode).toString(10) }</code></div> }
        </div>
    );
}

export default Code;