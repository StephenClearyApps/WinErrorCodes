import 'bluebird';
import 'whatwg-fetch';

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
