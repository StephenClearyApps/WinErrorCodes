﻿import React from 'react';
import { toInt16, valueAsInt16IsNegative, hex4 } from './helpers';

function Simple16BitCode({ code }: { code: number }) {
    return (
        <span>
            <code key='hex'>0x{ hex4(code) }</code> (<code key='dec'>{ code.toString(10) }</code>) { valueAsInt16IsNegative(code) ? [' (', <code key='dec16'>{ toInt16(code).toString(10) }</code>, ')'] : null }
        </span>
    );
}

export default Simple16BitCode;