import { bindActionCreators, IDispatch } from 'redux';
import { fetchJson } from './helpers';
import { ActionTypes, dataLoadDone, dataLoadError } from './action-types';
import { store } from './store';

function transformErrorMessage(errorMessage: ErrorMessageDto, type: ErrorMessageType): ErrorMessage {
    return {
        code: errorMessage.c,
        identifiers: errorMessage.i,
        text: errorMessage.t,
        type
    };
}

function transformFacilityName(facilityName: FacilityNameDto): FacilityName {
    return {
        name: facilityName.n,
        range: transformRange(facilityName.r),
        notes: facilityName.o
    }
}

function transformFacility(facility: FacilityDto): Facility {
    return {
        value: facility.v,
        names: facility.n.map(transformFacilityName)
    };
}

function transformRange(range: CodeRangeDto): CodeRange {
    return {
        description: range.d,
        lowerBound: range.l,
        upperBound: range.u,
        childRanges: range.c.map(transformRange)
    };
}

function transformData(data: DataDto): Data {
    return {
        win32: data.w.map(x => transformErrorMessage(x, ErrorMessageType.Win32)),
        win32Range: transformRange(data.wr),
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