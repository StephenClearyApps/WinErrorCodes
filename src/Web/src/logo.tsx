import React from 'react';
import { ErrorMessageType, errorMessageTypeHumanReadableString, errorMessageTypeHumanReadableShortString } from './typings/data';

function Logo({ type, full, style }: { type: ErrorMessageType, full: boolean, style?: React.CSSProperties }) {
    return (
        <div style={style} className={errorMessageTypeHumanReadableString(type).toLowerCase() + ' logo'}>
            <div>
                { full ? errorMessageTypeHumanReadableString(type) : errorMessageTypeHumanReadableShortString(type) }
            </div>
        </div>
    );
}

export default Logo;