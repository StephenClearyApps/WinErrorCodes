import * as _ from 'lodash';
import * as React from 'react';
import { connect } from 'react-redux';
import { IDispatch } from 'redux';
import { RoutedState } from './reducer';
import { search, QueryType } from './logic';

// http://www.2ality.com/2012/02/js-integers.html
function toInt32(x: number): number {
    if (x >= Math.pow(2, 31)) {
        return x - Math.pow(2, 32)
    } else {
        return x;
    }
}

function Analyze({ data, type, code }: { data: Data, type: QueryType, code: number }) {
    // Look up exact match if found.
    let result: ErrorMessage;
    if (type === 'win32') {
        result = _.find(data.win32, x => x.code === code);
    } else if (type === 'hresult') {
        result = _.find(data.hresult, x => x.code === code);
    } else if (type === 'ntstatus') {
        result = _.find(data.ntStatus, x => x.code === code);
    }

    let exactMatchDiv: JSX.Element;
    if (result) {
        exactMatchDiv = (
            <div>
                {result.identifiers.map(x => <div>{x}</div>) }
                <div>{result.text}</div>
            </div>
        );
    } else {
        exactMatchDiv = <div>No exact match found.</div>;
    }

    return (
        <div>
            <div>0x{code.toString(16)}</div>
            <div>{code.toString(10) }</div>
            <div>{toInt32(code).toString(10) }</div>
            {exactMatchDiv}
        </div>
    );
}

export default Analyze;