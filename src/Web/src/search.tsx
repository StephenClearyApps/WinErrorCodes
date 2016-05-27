import * as React from 'react';
import { browserHistory } from 'react-router';
import Helmet from 'react-helmet';
import { RoutedState } from './reducer';
import { ErrorMessage } from './typings/data';
import { search } from './logic';
import SearchResult from './search-result';

function Search({ data, location, dispatch }: RoutedState) {
    const { query }: any = location;
    const results = search(query.q, null, data);
    return (
        <div>
            <Helmet title='HRESULT, Win32, and NTSTATUS Error Codes'/>
            <div>
                <input type='text' value={query.q} onChange={e => browserHistory.replace('/?q=' + encodeURIComponent((e.target as HTMLInputElement).value))} />
            </div>
            <ul>
                {results.map(x => <SearchResult errorMessage={x} key={x.type + ':' + x.code} />)}
            </ul>
        </div>
    );
}

export default Search;