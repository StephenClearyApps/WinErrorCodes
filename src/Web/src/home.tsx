import * as React from 'react';
import { connect } from 'react-redux';
import { State } from './reducer';

function Home({ data, error }: State) {
    if (error) {
        return <div>Sorry, an error has occurred: {error.message}</div>;
    } else if (!data) {
        return <div>Loading...</div>;
    }
    return (
        <div>
            <div>{data.hresult.length} unique HRESULT values.</div>
            <div>{data.win32.length} unique Win32 values.</div>
            <div>{data.ntStatus.length} unique NTSTATUS values.</div>
            <div>{data.hresult.length + data.win32.length + data.ntStatus.length} total unique values.</div>
        </div>
    );
}

export default connect(x => x)(Home);