export const ActionTypes = {
    DATA_LOAD_DONE: 'DATA_LOAD_DONE',
    DATA_LOAD_ERROR: 'DATA_LOAD_ERROR'
};

export type DataLoadDoneAction = PayloadAction<Data>;
export function dataLoadDone(data: Data): DataLoadDoneAction {
    return {
        type: ActionTypes.DATA_LOAD_DONE,
        payload: data
    };
}

export type DataLoadErrorAction = ErrorAction;
export function dataLoadError(err: Error): DataLoadErrorAction {
    return {
        type: ActionTypes.DATA_LOAD_ERROR,
        payload: err,
        error: true
    };
}
