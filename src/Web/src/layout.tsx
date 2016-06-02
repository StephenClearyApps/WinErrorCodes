import React from 'react';
import { connect } from 'react-redux';
import Home from './home';
import Ad from './ad';

function Layout(props: any) {
    return (
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
    );
}

export default connect(x => x)(Layout);