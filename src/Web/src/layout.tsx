import React from 'react';
import { connect } from 'react-redux';
import { Link } from 'react-router';
import Home from './home';
import Ad from './ad';

function Layout(props: any) {
    return (
        <div>
            <nav className="navbar navbar-default navbar-fixed-top">
                <div className="container-fluid">
                    <div className="navbar-header">
                        <button type="button" className="navbar-toggle collapsed" data-toggle="collapse" data-target="#bs-example-navbar-collapse-1" aria-expanded="false">
                            <span className="sr-only">Toggle navigation</span>
                            <span className="icon-bar"></span>
                            <span className="icon-bar"></span>
                            <span className="icon-bar"></span>
                        </button>
                        <Link className="navbar-brand" to="/">ErrorCodeLookup.com</Link>
                    </div>
                    <div className="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                        <ul className="nav navbar-nav navbar-right">
                            <li><a href="http://stephencleary.com/">Author</a></li>
                        </ul>
                    </div>
                </div>
            </nav>
            <div className='container-fluid'>
                <div className='row'>
                    <div className='col-sm-5 col-md-4'>
                        <Ad slot='7090994222' width={300} height={600}/>
                    </div>
                    <div className='col-sm-2 col-md-4'>
                        <Home {...props}/>
                    </div>
                    <div className='col-sm-5 col-md-4'>
                        <Ad slot='1044460625' width={300} height={600}/>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default connect(x => x)(Layout);