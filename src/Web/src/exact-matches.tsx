import React from 'react';
import ExactMatch from './exact-match';
import { ErrorMessage } from './typings/data';

function ExactMatches({ errorMessages }: { errorMessages: ErrorMessage[] }) {
    if (errorMessages.length) {
        return (
            <div>
                { errorMessages.map(x => <ExactMatch errorMessage={x} key={x.code} />) }
            </div>
        );
    }
    return (
        <div>
            <div>No exact match found.</div>
        </div>
    );
}

export default ExactMatches;