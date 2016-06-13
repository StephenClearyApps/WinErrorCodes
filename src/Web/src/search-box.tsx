import React from 'react';
import { browserHistory } from 'react-router';

function onChange(e: React.FormEvent) {
    const target = e.target as HTMLInputElement;
    browserHistory.replace('/?q=' + encodeURIComponent(target.value));
}

function SearchBox({ query = '', placeholder = 'Search' }: { query?: string, placeholder?: string }) {
    return (
        <div className='form-group'>
            <input type='text' className='form-control' value={query} onChange={onChange} placeholder={placeholder} />
        </div>
    );
}

export default SearchBox;