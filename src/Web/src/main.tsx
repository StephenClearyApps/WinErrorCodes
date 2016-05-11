import 'babel-polyfill';
import 'bluebird';
import 'whatwg-fetch';
import * as React from 'react';
import { render } from 'react-dom';
import { Provider } from 'react-redux';
import { Router, IndexRoute, Route, browserHistory } from 'react-router';
import Home from './home';
import { store } from './store';
import { Actions } from './actions';

Actions.loadData('data.json');
window.onload = () => {
    render((
        <Provider store={store}>
            <Router history={browserHistory}>
                <Route path='/'>
                    <IndexRoute component={Home}/>
                </Route>
            </Router>
        </Provider>
    ), document.getElementById("app"));
};
