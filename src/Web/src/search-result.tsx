import * as React from 'react';
import { Link } from 'react-router';
import { ErrorMessage, ErrorMessageType, errorMessageTypeHumanReadableString } from './typings/data';
import { hex8 } from './helpers';
import { errorMessageUrl } from './logic';

function humanReadableCode(errorMessage: ErrorMessage): string {
    if (errorMessage.type === ErrorMessageType.Win32) {
        return errorMessage.code.toString(10);
    }
    return '0x' + hex8(errorMessage.code);
}

function SearchResult({ errorMessage }: { errorMessage: ErrorMessage }) {
    return (
        <Link to={errorMessageUrl(errorMessage)} className='list-group-item'>
            {errorMessage.identifiers.length ? <h4><code>{errorMessage.identifiers[0]}</code></h4> : null}
            <h4><code>{humanReadableCode(errorMessage)}</code> ({errorMessageTypeHumanReadableString(errorMessage.type)})</h4>
            <p className='truncated'>{errorMessage.text}</p>
        </Link>
    );
}

export default SearchResult;