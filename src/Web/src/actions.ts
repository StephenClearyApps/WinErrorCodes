import { bindActionCreators, IDispatch } from 'redux';
import { fetchJson } from './helpers';
import { ActionTypes, dataLoadDone, dataLoadError } from './action-types';
import { store } from './store';
import { transformData, DataDto } from './typings/data';

const actionCreators = {
    loadData: (url: string) => (dispatch: IDispatch) => {
        return fetchJson<DataDto>(url)
            .then(data => dispatch(dataLoadDone(transformData(data))))
            .catch(err => dispatch(dataLoadError(err)));
    }
}

export const Actions = bindActionCreators(actionCreators as any, store.dispatch) as typeof actionCreators;