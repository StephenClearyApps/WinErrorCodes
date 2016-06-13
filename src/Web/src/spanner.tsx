import React from 'react';
import _ from 'lodash';
import { toInt32, toInt16, valueAsInt32IsNegative, valueAsInt16IsNegative, hex4, hex8 } from './helpers';

type Range = {
    begin: number;
    end: number;
};

function Spanner({ text, ranges }: { text: string, ranges: Range[] }) {
    const result = [];
    let textIndex = 0;
    let rangeIndex = 0;
    while (textIndex < text.length) {
        if (rangeIndex >= ranges.length) {
            result.push(<span className='highlightable' key={textIndex}>{text.substring(textIndex)}</span>);
            break;
        }

        const range = ranges[rangeIndex];
        if (textIndex !== range.begin) {
            result.push(<span className='highlightable' key={textIndex}>{text.substring(textIndex, range.begin)}</span>);
            textIndex = range.begin;
        }

        result.push(<span className='highlightable highlight' key={textIndex}>{text.substring(range.begin, range.end)}</span>);
        textIndex = range.end;
        ++rangeIndex;
    }
    return <span>{result}</span>;
}

export default Spanner;