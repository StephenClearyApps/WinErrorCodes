import React from 'react';
import { toInt32, toInt16, valueAsInt32IsNegative, valueAsInt16IsNegative, hex4, hex8 } from './helpers';

function Code({ code }: { code: number }) {
    const errorCode = ((code & 0xFFFF0000) != 0) ? (code & 0xFFFF) : null;
    return (
        <div>
            <h3>Numeric Values</h3>
            <table className='table'>
                <thead>
                    <tr><th>Value</th><th>Type</th></tr>
                </thead>
                <tbody>
                    <tr><td><code>0x{ hex8(code) }</code></td><td>Hexadecimal 32-bit unsigned integer</td></tr>
                    <tr><td><code>{ code.toString(10) }</code></td><td>Decimal 32-bit unsigned integer</td></tr>
                    {
                        valueAsInt32IsNegative(code) ?
                            <tr><td><code>{ toInt32(code).toString(10) }</code></td><td>Decimal 32-bit signed integer</td></tr> :
                            null
                    }
                    {
                        errorCode === null ? null :
                            <tr><td><code>0x{ hex4(errorCode) }</code></td><td>Hexadecimal 16-bit unsigned integer</td></tr>
                    }
                    {
                        errorCode === null ? null :
                            <tr><td><code>{ errorCode.toString(10) }</code></td><td>Decimal 16-bit unsigned integer</td></tr>
                    }
                    {
                        errorCode === null || !valueAsInt16IsNegative(errorCode) ? null :
                            <tr><td><code>{ toInt16(errorCode).toString(10) }</code></td><td>Decimal 16-bit signed integer</td></tr>
                    }
                </tbody>
            </table>
        </div>
    );
}

export default Code;