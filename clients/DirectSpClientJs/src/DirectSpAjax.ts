import { DirectSpError } from "./DirectSpError";
import { Convert, IDirectSpKeyToAny } from "./DirectSpUtil";

export interface IDirectSpRequest {
    url: string;
    method: string;
    data?: any;
    withCredentials?: boolean;
    headers?: IDirectSpKeyToAny;
    cache?: boolean;
}

export interface IDirectSpResponse {
    data: any;
    headers: any;
}

export interface IDirectSpAjaxProvider {
    fetch(request: IDirectSpRequest): Promise<IDirectSpResponse>;
}

export class DirectSpXmlHttpAjaxProvider implements IDirectSpAjaxProvider {

    fetch(request: IDirectSpRequest): Promise<IDirectSpResponse> {
        return new Promise((resolve, reject) => {
            let req = new XMLHttpRequest();
            req.withCredentials = Convert.toBoolean(request.withCredentials, false);
            req.open(request.method, request.url);
            req.onload = () => {

                if (req.status == 200) {
                    const response: IDirectSpResponse = { data: req.responseText, headers: req.getResponseHeader };
                    resolve(response);
                } else {
                    let error = null;
                    try {
                        let obj = JSON.parse(req.responseText);
                        error = DirectSpError.create(obj);
                        error.innerError = obj;
                    } catch (err) {
                        let text = req.responseText == "" ? req.statusText : req.responseText;
                        error = DirectSpError.create(text);
                    }

                    error.status = req.status;
                    error.statusText = req.statusText;
                    reject(error);
                }
            };
            req.onerror = () => {
                const error = new DirectSpError("Network error or server unreachable!");
                error.errorName = "Network Error";
                error.errorNumber = 503;
                reject(error);
            };

            //headers
            if (request.headers) {
                for (let item in request.headers) {
                    if (request.headers.hasOwnProperty(item))
                        req.setRequestHeader(item, request.headers[item]);
                }
            }

            //creating body
            let body = request.data;

            //finding Content-Type
            let contentType = request.headers
                ? request.headers["Content-Type"]
                : null;
            if (!contentType)
                contentType = "application/x-www-form-urlencoded;charset=UTF-8"; //default
            contentType = contentType.toLowerCase();

            //convert data based on contentType
            if (contentType.indexOf("application/json") != -1 && body)
                body = JSON.stringify(request.data);
            else if (contentType.indexOf("application/x-www-form-urlencoded") != -1)
                body = Convert.toQueryString(request.data);

            //send
            req.send(body);
        });
    }

}

