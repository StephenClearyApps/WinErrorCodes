import React from 'react';
import * as dataTypings from './typings/data';
import { ErrorMessage } from './typings/data';

// Weird dataTypings import and any cast: https://github.com/Microsoft/TypeScript/issues/3422

function ExactMatch({ errorMessage }: { errorMessage: ErrorMessage }) {
    return (
        <div>
            <div>Type: {(dataTypings as any).ErrorMessageType[errorMessage.type]}</div>
            <div>{errorMessage.identifiers.map(x => <div key={x}><code>{x}</code></div>) }</div>
            <div>{errorMessage.text}</div>
        </div>
    );
}

export default ExactMatch;