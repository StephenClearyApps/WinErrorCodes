import React from 'react';
import { ErrorMessage, errorMessageTypeHumanReadableString } from './typings/data';
import Logo from './logo';

function ExactMatch({ errorMessage }: { errorMessage: ErrorMessage }) {
    return (
        <div className='lead'>
            <Logo full={true} type={errorMessage.type} style={{float: 'left', marginRight: '0.2em'}}/>
            <div>{errorMessage.identifiers.map(x => <div key={x}><code>{x}</code></div>) }</div>
            <div>{errorMessage.text}</div>
        </div>
    );
}

export default ExactMatch;