import React from 'react';
import ExactMatch from './exact-match';
import { ErrorMessage } from './typings/data';

function ExactMatches({ errorMessages }: { errorMessages: ErrorMessage[] }) {
    if (errorMessages.length) {
        return (
            <div>
                { errorMessages.map(x => <ExactMatch errorMessage={x} key={x.type + ' ' + x.code} />) }
            </div>
        );
    }
    return (
        <div className='panel panel-default'>
            <div className='panel-body'>
                <div>No exact match found.</div>
            </div>
        </div>
    );
}

export default ExactMatches;