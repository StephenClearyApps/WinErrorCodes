import React from 'react';
import { connect } from 'react-redux';
import Home from './home';
import Ad from './ad';

function Layout(props: any) {
    return (
        <div className='container-fluid'>
            <div className='row'>
                <div className='col-sm-5 col-md-4'>
                    <Ad slot='9313090625'/>
                </div>
                <div className='col-sm-2 col-md-4'>
                    <Home {...props}/>
                </div>
                <div className='col-sm-5 col-md-4'>
                    <Ad slot='1789823825'/>
                </div>
            </div>
        </div>
    );
}

// <Ad slot='6220023426' width:728 height:90/>

export default connect(x => x)(Layout);