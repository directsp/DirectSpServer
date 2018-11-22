import { DirectSpError } from "./DirectSpError";
import { IDirectSpRequest, IDirectSpResponse } from "./DirectSpAjax";
import { DirectSpClient, IDirectSpInvokeOptions } from "./DirectSpClient";
import { Utility } from "./DirectSpUtil";


export interface IDirectSpErrorControllerOptions {
    error: DirectSpError;
    dspClient: DirectSpClient;
    request?: IDirectSpRequest;
}

export class DirectSpErrorController {
    public captchaCode: string | null = null;
    private _data: IDirectSpErrorControllerOptions;
    private _error: DirectSpError;
    private _errorNumber: number | null;
    private readonly _canRetry: boolean;
    private readonly _captchaImageUri: string | null = null;
    private readonly _promise: Promise<IDirectSpResponse>;
    private _reject: ((error: DirectSpError) => void) | null = null;
    private _resolve: ((data: IDirectSpResponse) => void) | null = null;

    constructor(data: IDirectSpErrorControllerOptions) {
        this._data = data;
        this._error = data.error;
        this._errorNumber = data.error ? data.error.errorNumber : null;

        //55022: InvalidCaptcha, 55027: Maintenance, 55028: MaintenanceReadOnly
        this._canRetry =
            this._errorNumber == 55022 ||
            this._errorNumber == 55027 ||
            this._errorNumber == 55028 ||
            this._errorNumber == 503;

        if (this._errorNumber == 55022) {
            this._captchaImageUri =
                "data:image/png;base64," + data.error.errorData.captchaImage;
            this.captchaCode = null;
        }

        this._promise = new Promise<IDirectSpResponse>((resolve, reject) => {
            this._resolve = resolve;
            this._reject = reject;
            if (!this._canRetry)
                reject(data.error);
        });
    };

    public get promise(): Promise<IDirectSpResponse> { return this._promise; }
    public get error(): DirectSpError { return this._error; }
    public get captchaImageUri(): string | null { return this._captchaImageUri; }
    public get canRetry(): boolean { return this._canRetry; }
    public get dspClient(): DirectSpClient { return this._data.dspClient; }

    public retry(): void {
        const request: IDirectSpRequest | undefined = this._data.request;

        if (!this.canRetry || !request)
            throw new DirectSpError("Can not retry this error!");


        if (request && this._errorNumber == 55022) {
            const requestData: any =  request.data;
            let invokeOptions = request.data && request.data.invokeOptions ? <IDirectSpInvokeOptions>request.data.invokeOptions : null;
    
            //try update invokeParams for invoke
            if (invokeOptions && this.error.errorData) {
                invokeOptions.captchaId = this.error.errorData.captchaId;
                invokeOptions.captchaCode = <string>this.captchaCode;
                invokeOptions.requestId = Utility.generateGuid();
            }

            //try update param for authentication grantby password
            if (requestData.grant_type == "password") {
                requestData.captchaId = this.error.errorData.captchaId;
                requestData.captchaCode = this.captchaCode;
            }
        }

        // retry original ajax
        this.dspClient
            ._fetch(request)
            .then(resolve => {
                if (!this._resolve)
                    throw new DirectSpError("Object is not ready yet!");
                return this._resolve(resolve);
            })
            .catch(error => {
                if (!this._reject)
                    throw new DirectSpError("Object is not ready yet!");
                return this._reject(error);
            });
    };

    public release(): void {
        if (!this._reject)
            throw new DirectSpError("Object is not ready yet!");
        this._reject(this._error);
    };
}


