import * as React from 'react';
import { RoutedState } from './reducer';
import { search, QueryType } from './logic';
import Analyze from './analyze';
import Search from './search';

function Home(state: RoutedState) {
    const { data, error, location } = state;

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

    return <Search {...state} />;
}

export default Home;