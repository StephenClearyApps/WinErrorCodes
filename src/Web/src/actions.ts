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

function transformErrorMessage(errorMessage: ErrorMessageDto, type: ErrorMessageType): ErrorMessage {
    return {
        code: errorMessage.c,
        identifiers: errorMessage.i,
        text: errorMessage.t,
        type
    };
}

function transformFacility(facility: FacilityDto): Facility {
    return {
        value: facility.v,
        names: facility.n
    };
}

function transformData(data: DataDto): Data {
    return {
        win32: data.w.map(x => transformErrorMessage(x, ErrorMessageType.Win32)),
        ntStatus: data.n.map(x => transformErrorMessage(x, ErrorMessageType.NtStatus)),
        hresult: data.h.map(x => transformErrorMessage(x, ErrorMessageType.HResult)),
        ntStatusFacilities: data.nf.map(transformFacility),
        hresultFacilities: data.hf.map(transformFacility)
    };
}

const actionCreators = {
    loadData: (url: string) => (dispatch: IDispatch) => {
        return fetchJson<DataDto>(url)
            .then(data => dispatch(dataLoadDone(transformData(data))))
            .catch(err => dispatch(dataLoadError(err)));
    }
}

export const Actions = bindActionCreators(actionCreators as any, store.dispatch) as typeof actionCreators;