import * as React from 'react';
import { Data } from './typings/data';
import { QueryType, findCodes } from './logic';
import ExactMatches from './exact-matches';
import Code from './code';
import AnalyzeHResult from './analyze-hresult';

function Analyze({ data, type, code }: { data: Data, type: QueryType, code: number }) {
    // Look up exact match if found.
    const errorMessages = findCodes(data, type, code);
    return (
        <div>
            <Code code={code} />
            <ExactMatches errorMessages={errorMessages} />
            { type === 'hresult' ? <AnalyzeHResult data={data} code={code}/> : null }
        </div>
    );

    //{ type === 'hresult' ? <AnalyzeHResult/> : null }
    //{ type === 'ntstatus' ? <AnalyzeNTStatus/> : null }
}

export default Analyze;