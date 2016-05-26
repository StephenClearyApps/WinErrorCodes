import * as _ from 'lodash';
import * as React from 'react';
import { Data } from './typings/data';
import { QueryType, findCodes, hresultUnwrapWin32 } from './logic';
import { hex8 } from './helpers';
import ExactMatches from './exact-matches';
import Code from './code';
import AnalyzeHResult from './analyze-hresult';
import HelmetDisqus from './helmet-disqus';

function Analyze({ data, type, code }: { data: Data, type: QueryType, code: number }) {
    // Look up exact match if found.
    const errorMessages = findCodes(data, type, code);
    return (
        <div>
            <Code code={code} />
            <ExactMatches errorMessages={errorMessages} />
            { type === 'hresult' ? <AnalyzeHResult data={data} code={code}/> : null }
            <HelmetDisqus type={type} code={code} errorMessages={errorMessages} />
        </div>
    );

    //{ type === 'hresult' ? <AnalyzeHResult/> : null }
    //{ type === 'ntstatus' ? <AnalyzeNTStatus/> : null }
}

export default Analyze;