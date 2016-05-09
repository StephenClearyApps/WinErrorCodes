import { createStore, applyMiddleware, compose } from 'redux';
import thunk from 'redux-thunk';
import { handleActions } from 'redux-actions';
import { ActionTypes } from './actions';

const reducer = handleActions({}, {});

const middlewareStoreEnhancer = applyMiddleware(thunk);
const devToolsEnhancer = (DEBUG && (window as any).devToolsExtension) ? (window as any).devToolsExtension() : undefined;
const storeEnhancer = devToolsEnhancer ? compose(middlewareStoreEnhancer, devToolsEnhancer) : middlewareStoreEnhancer;

export const store = storeEnhancer(createStore)(reducer);
