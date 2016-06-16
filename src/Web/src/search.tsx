import React from 'react';
import { browserHistory } from 'react-router';
import Helmet from 'react-helmet';
import { RoutedState } from './reducer';
import { ErrorMessage } from './typings/data';
import { search, isValidTextQuery } from './logic';
import SearchBox from './search-box';
import SearchResult from './search-result';

function Search({ data, location }: RoutedState) {
    const locationQuery: any = location.query;
    const query = locationQuery.q || '';
    const results = search(query, null, data);
    let resultList = <div>No matches found.</div>;
    if (results.length) {
        resultList = (
            <div className='list-group'>
                {results.map(x => <SearchResult errorMessage={x} key={x.type + ':' + x.code} />) }
            </div>
        );
    } else if (!isValidTextQuery(query)) {
        resultList = <div>Type a longer search query to see results.</div>;
    }
    return (
        <div>
            <Helmet title='HRESULT, Win32, and NTSTATUS Error Codes'/>
            <div className='jumbotron'>
                <img src='icon-128x128.png' style={{width:128, height:128, float:'left', margin:'10px 10px 10px 0px'}} />
                <h1>Error&#8203;Code&#8203;Lookup&#8203;.com</h1>
                <p>Your source for the meanings of HRESULT, Win32, and NTSTATUS error codes!</p>
            </div>
            <form role='search'>
                <SearchBox query={query} placeholder='Search error codes and messages' />
            </form>
            {resultList}
        </div>
    );
}

export default Search;