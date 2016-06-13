import React from 'react';
import _ from 'lodash';
import { toInt32, toInt16, valueAsInt32IsNegative, valueAsInt16IsNegative, hex4, hex8 } from './helpers';

type Range = {
    begin: number;
    end: number;
};

function Spanner({ text, ranges, className }: { text: string, ranges: Range[], className?: string }) {
    className = className || 'highlight';
    const result = [];
    let textIndex = 0;
    let rangeIndex = 0;
    while (textIndex < text.length) {
        if (rangeIndex >= ranges.length) {
            result.push(text.substring(textIndex));
            break;
        }

        const range = ranges[rangeIndex];
        if (textIndex !== range.begin) {
            result.push(text.substring(textIndex, range.begin));
        }

        result.push(<span className={className} key={range.begin}>{text.substring(range.begin, range.end)}</span>);
        textIndex = range.end;
        ++rangeIndex;
    }
    return <span>{result}</span>;
}

export default Spanner;