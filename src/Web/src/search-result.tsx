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
        <li>
            <Link to={errorMessageUrl(errorMessage)}>
                {errorMessageTypeHumanReadableString(errorMessage.type)}: {humanReadableCode(errorMessage)}: {errorMessage.identifiers.length ? errorMessage.identifiers[0] : null}: {errorMessage.text}
            </Link>
        </li>
    );
}

export default SearchResult;