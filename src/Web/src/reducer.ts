import { handleActions } from 'redux-actions';
import { spread } from './helpers';
import { Data } from './typings/data';
import { ActionTypes, DataLoadDoneAction, DataLoadErrorAction } from './action-types';
import { Location } from '~react-router~history/history';

export interface State {
    error: Error;
    data: Data;
}

export interface RoutedState extends State {
    location: Location;
}

const defaultState: State = {
    error: null,
    data: null
};

function dataLoadDone(state: State, action: DataLoadDoneAction): State {
    const result = spread(state);
    result.data = action.payload;
    return result;
}

function dataLoadError(state: State, action: DataLoadErrorAction): State {
    const result = spread(state);
    result.error = action.payload;
    return result;
}

export const reducer = (handleActions as ReduxActionsFixed.HandleActions<State>)({
    [ActionTypes.DATA_LOAD_DONE]: dataLoadDone,
    [ActionTypes.DATA_LOAD_ERROR]: dataLoadError
}, defaultState);
