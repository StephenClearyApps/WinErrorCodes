import React from 'react';
import { ErrorMessage } from './typings/data';
import { hex8 } from './helpers';
import Logo from './logo';

function ExactMatch({ errorMessage }: { errorMessage: ErrorMessage }) {
    const errorIdentifier = errorMessage.identifiers.length ? errorMessage.identifiers[0] : null;
    const title = errorIdentifier || ('0x' + hex8(errorMessage.code));
    return (
        <div className='panel panel-default'>
            <div className='panel-body'>
                <Logo full={true} type={errorMessage.type} style={{display:'block'}}/>
                <h3><code>{title}</code></h3>
                <div>{errorMessage.text}</div>
                {
                    errorMessage.identifiers.length > 1 ?
                        <div><h3>Symbolic Identifiers</h3>{errorMessage.identifiers.map(x => <div key={x}><code>{x}</code></div>) }</div> :
                        null
                }
            </div>
        </div>
    );
}

export default ExactMatch;