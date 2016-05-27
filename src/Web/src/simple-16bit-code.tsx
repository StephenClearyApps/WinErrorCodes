import * as React from 'react';
import { toInt16, valueAsInt16IsNegative, hex4 } from './helpers';

function Simple16BitCode({ code }: { code: number }) {
    return <span>0x{ hex4(code) } ({ code.toString(10) }){ valueAsInt16IsNegative(code) ? ' (' + toInt16(code).toString(10) + ')' : null }</span>;
}

export default Simple16BitCode;