import * as React from 'react';
import { connect } from 'react-redux';
import { IDispatch } from 'redux';
import { RoutedState } from './reducer';
import { search, QueryType } from './logic';
import Analyze from './analyze';

function Home({ data, error, location }: RoutedState, dispatch: IDispatch) {
    if (error) {
        return <div>Sorry, an error has occurred: {error.message}</div>;
    }

    if (!data) {
        return <div>Loading...</div>;
    }

    const { query }: any = location;
    if (query && query.code && query.type) {
        const code = parseInt(query.code, 16);
        return <Analyze code={code} data={data} type={query.type} />;
     }

    return (
        <div>
            <div>
                <input type='text' onChange={e => console.log((e.target as HTMLInputElement).value)} />
            </div>
            <div>
                <div>{data.hresult.length} unique HRESULT values.</div>
                <div>{data.win32.length} unique Win32 values.</div>
                <div>{data.ntStatus.length} unique NTSTATUS values.</div>
                <div>{data.hresult.length + data.win32.length + data.ntStatus.length} total unique values.</div>
            </div>
        </div>
    );
}

export default connect(x => x)(Home);