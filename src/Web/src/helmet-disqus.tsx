import _ from 'lodash';
import React from 'react';
import { ErrorMessage } from './typings/data';
import { QueryType, uniqueIdentifier } from './logic';
import { hex8 } from './helpers';
import DisqusThread from 'react-disqus-thread';
import Helmet from 'react-helmet';

function HelmetDisqus({ type, code, errorMessages }: { type: QueryType, code: number, errorMessages: ErrorMessage[] }) {
    const errorMessageWithIdentifier = _.find(errorMessages, x => x.identifiers.length);
    const errorIdentifier = errorMessageWithIdentifier ? errorMessageWithIdentifier.identifiers[0] : null;
    const title = errorIdentifier ? errorIdentifier :
        type === 'win32' ? 'Win32 Error Code ' + code.toString(10) :
            type === 'hresult' ? 'HRESULT Error Code 0x' + hex8(code) :
                'NTSTATUS Error Code 0x' + hex8(code);
    return (
        <div>
            <Helmet title={title}/>
            <DisqusThread shortname="errorcodelookup"
                identifier={'test-' + uniqueIdentifier(type, code)}
                title={title}
                url={window.location.href.replace(new RegExp('//[a-z.]+(:[0-9]+)?/'), '//errorcodelookup.com/')} />
        </div>
    );
}

export default HelmetDisqus;