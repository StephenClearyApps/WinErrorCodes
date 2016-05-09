import { bindActionCreators, IDispatch } from 'redux';
import { fetchJson } from './helpers';
import { store } from './store';

export const ActionTypes = {
    DATA_LOAD_DONE: 'DATA_LOAD_DONE',
    DATA_LOAD_ERROR: 'DATA_LOAD_ERROR'
};

export type DataLoadDoneAction = PayloadAction<Data>;
function dataLoadDone(data: Data): DataLoadDoneAction {
    return {
        type: ActionTypes.DATA_LOAD_DONE,
        payload: data
    };
}

export type DataLoadErrorAction = ErrorAction;
function dataLoadError(err: Error): DataLoadErrorAction {
    return {
        type: ActionTypes.DATA_LOAD_ERROR,
        payload: err,
        error: true
    };
}

const actionCreators = {
    loadData: (url: string) => (dispatch: IDispatch) => {
        return fetchJson<Data>(url)
            .then(data => dispatch(dataLoadDone(data)))
            .catch(err => dispatch(dataLoadError(err)));
    }
}

export const Actions = bindActionCreators(actionCreators as any, store.dispatch) as typeof actionCreators;