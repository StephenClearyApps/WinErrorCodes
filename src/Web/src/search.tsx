import React from 'react';
import { browserHistory } from 'react-router';
import Helmet from 'react-helmet';
import { RoutedState } from './reducer';
import { ErrorMessage } from './typings/data';
import { search, isValidTextQuery } from './logic';
import SearchResult from './search-result';

function Search({ data, location }: RoutedState) {
    const { query }: any = location;
    const q = query.q || '';
    const results = search(q, null, data);
    let resultList = <div>No matches found.</div>;
    if (results.length) {
        resultList = (
            <div className='list-group'>
                {results.map(x => <SearchResult errorMessage={x} key={x.type + ':' + x.code} />)}
            </div>
        );
    } else if (!isValidTextQuery(q)) {
        resultList = <div>Type a longer search query to see results.</div>;
    }
    return (
        <div>
            <Helmet title='HRESULT, Win32, and NTSTATUS Error Codes'/>
            <div>
                <input type='text' value={query.q || ''} onChange={e => browserHistory.replace('/?q=' + encodeURIComponent((e.target as HTMLInputElement).value))} />
            </div>
            {resultList}
        </div>
    );
}

export default Search;