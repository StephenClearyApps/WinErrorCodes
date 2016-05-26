import * as _ from 'lodash';
import * as React from 'react';
import { Data } from './typings/data';
import { QueryType, findCodes } from './logic';
import { hex8 } from './helpers';
import ExactMatches from './exact-matches';
import Code from './code';
import AnalyzeHResult from './analyze-hresult';
import ReactDisqusThread from 'react-disqus-thread';
import Helmet from 'react-helmet';

function Analyze({ data, type, code }: { data: Data, type: QueryType, code: number }) {
    // Look up exact match if found.
    const errorMessages = findCodes(data, type, code);
    const errorMessageIdentifier = _.find(errorMessages, x => x.identifiers.length);
    const errorIdentifier = errorMessageIdentifier ? errorMessageIdentifier.identifiers[0] : null;
    const title = errorIdentifier ? errorIdentifier : 'HRESULT Error Code ' + hex8(code);
    return (
        <div>
            <Helmet title={title}/>
            <Code code={code} />
            <ExactMatches errorMessages={errorMessages} />
            { type === 'hresult' ? <AnalyzeHResult data={data} code={code}/> : null }
            <ReactDisqusThread shortname="errorcodelookup"
                identifier={'test-' + 'h' + hex8(code)}
                title={title}
                url={window.location.href.replace(new RegExp('//[a-z.]+(:[0-9]+)?/'), '//errorcodelookup.com/')} />
        </div>
    );

    //{ type === 'hresult' ? <AnalyzeHResult/> : null }
    //{ type === 'ntstatus' ? <AnalyzeNTStatus/> : null }
}

export default Analyze;