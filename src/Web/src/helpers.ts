function checkStatus(response: Response): Response {
    if (response.status >= 200 && response.status < 300) {
        return response;
    } else {
        const error = new Error(response.statusText);
        (error as any).response = response;
        throw error;
    }
}

function parseJson(response: Response): Promise<any> {
    return response.json();
}

export function fetchJson<T>(url: string): Promise<T> {
    return window.fetch(url)
        .then(checkStatus)
        .then(parseJson);
}
