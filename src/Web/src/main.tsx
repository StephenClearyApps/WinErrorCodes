import 'babel-polyfill';
import 'bluebird';
import 'whatwg-fetch';
import * as React from 'react';
import { render } from 'react-dom';
import { createStore } from 'redux';
import { Provider } from 'react-redux';
import { handleActions } from 'redux-actions';
import { Router, IndexRoute, Route, browserHistory } from 'react-router';
import { Home } from './home';

const reducer = handleActions({}, {});
const storeFactory = (DEBUG && (window as any).devToolsExtension) ? (window as any).devToolsExtension()(createStore) : createStore;
const store = storeFactory(reducer);

function checkStatus(response) {
    if (response.status >= 200 && response.status < 300) {
        return response;
    } else {
        const error = new Error(response.statusText);
        (error as any).response = response;
        throw error;
    }
}

function parseJSON(response) {
    return response.json();
}

$(() => {
    render((
        <Provider store={store}>
            <Router history={browserHistory}>
                <Route path='/'>
                    <IndexRoute component={Home as any}/>
                </Route>
            </Router>
        </Provider>
    ), $("#app")[0]);

    window.fetch('data.json')
        .then(checkStatus)
        .then(parseJSON)
        .then(data => {
            console.log(data);
        })
        .catch(error => {
            console.log(error); // TODO
        });
});
