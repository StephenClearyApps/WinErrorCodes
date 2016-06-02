import React from 'react';
import { ErrorMessage, errorMessageTypeHumanReadableString } from './typings/data';

function ExactMatch({ errorMessage }: { errorMessage: ErrorMessage }) {
    return (
        <div>
            <div>Type: {errorMessageTypeHumanReadableString(errorMessage.type)}</div>
            <div>{errorMessage.identifiers.map(x => <div key={x}><code>{x}</code></div>) }</div>
            <div>{errorMessage.text}</div>
        </div>
    );
}

export default ExactMatch;