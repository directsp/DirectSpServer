// import {directSp} from "./directSp";
import { IDirectSpRequest, IDirectSpResponse, IDirectSpAjaxProvider, DirectSpXmlHttpAjaxProvider } from "./DirectSpAjax";
import { DirectSpAuth, IDirectSpAuthOptions, IDirectSpAuthorizedData } from "./DirectSpAuth";
import { DirectSpError, exceptions } from "./DirectSpError";
import { DirectSpHtmlStorage, IDirectSpStorage } from "./DirectSpStorage";
import { Utility, Convert, Uri, IDirectSpKeyToAny } from "./DirectSpUtil";
import { DirectSpErrorController } from "./DirectSpErrorController";
import { DirectSpHelp } from "./DirectSpHelp";

export interface IDirectSpOptions {
    homePageUri?: string;
    resourceApiUri?: string;
    isAutoReload?: boolean;
    isLogEnabled?: boolean;
    isUseAppErrorHandler?: boolean;
    dspLocalStorage?: IDirectSpStorage;
    dspSessionStorage?: IDirectSpStorage;
    auth?: IDirectSpAuthOptions;
    ajaxProvider?: IDirectSpAjaxProvider;
}

export interface IDirectSpInvokeOptions {
    recordsetFormat?: string;
    recordIndex?: number;
    recordCount?: number;
    isWithRecodsetDownloadUri?: boolean;
    isUseAppErrorHandler?: boolean;
    autoDownload?: boolean;
    captchaId?: string;
    captchaCode?: string;
    requestId?: string;
    seqGroup?: string;
    cache?: boolean;
}

export interface IDirectSpCall {
    method: string;
    params?: IDirectSpKeyToAny;
}

export interface IDirectSpInvokeParams {
    spCall?: IDirectSpCall;
    spCalls?: IDirectSpCall[];
    invokeOptions?: IDirectSpInvokeOptions;
}

export interface IDirectSpInvokeResult extends IDirectSpKeyToAny {
    Recordset?: [];
    ReturnValue?: any;
    RecordsetUri?: string;
}


export interface IDirectSpHookParams {
    isHandled: boolean;
    invokeParams: IDirectSpInvokeParams;
    delay: number
}

/**
 * @see: https://github.com/directsp
 * @event onAuthorized: check the authorization state
 * @event onError: fire when an herror occurred
 * @event: onNewVersion: fire when new API version detected
 * @event: onBeforeInvoke: fire beofore any invoke
 */
export class DirectSpClient {
    public isAutoReload: boolean;
    public isLogEnabled: boolean;
    public isUseAppErrorHandler: boolean;
    private readonly _homePageUri: string | null;
    private readonly _storageNamePrefix: string = "DirectSp:";
    private readonly _resourceApiUri: string;
    private readonly _ajaxProvider: IDirectSpAjaxProvider;
    private readonly _auth: DirectSpAuth | null;
    private readonly _originalUri: string | null = null;
    private readonly _originalQueryString: string | null = null;
    private readonly _dspSessionStorage: IDirectSpStorage;
    private readonly _dspLocalStorage: IDirectSpStorage;
    private readonly _seqGroups: { [key: string]: number } = {};
    private _sessionState: string = Math.floor(Math.random() * 10000000000000).toString();
    private _resourceAppVersion: string | null = null;
    private _systemApi: any = null;

    //Events
    public onError: ((error: DirectSpErrorController) => void) | null = null;
    public onAuthorized: ((data: IDirectSpAuthorizedData) => void) | null = null;
    public onNewVersion: (() => void) | null = null;
    public onBeforeInvoke: ((hookParams: IDirectSpHookParams) => Promise<IDirectSpRequest>) | null = null;

    public constructor(options: IDirectSpOptions) {

        if (!options.dspLocalStorage) {
            if (!Utility.isHtmlHost)
                throw new DirectSpError("dspLocalStorage has not been set!");
            options.dspLocalStorage = new DirectSpHtmlStorage(window.localStorage);
        }

        if (!options.dspSessionStorage) {
            if (!Utility.isHtmlHost)
                throw new DirectSpError("dspSessionStorage has not been set!");
            options.dspSessionStorage = new DirectSpHtmlStorage(window.sessionStorage);
        }

        if (!options.resourceApiUri)
            throw new DirectSpError("resourceApiUri has not been set!");

        this._resourceApiUri = options.resourceApiUri;
        this._auth = options.auth && options.auth.clientId && options.auth.clientId != "" ? new DirectSpAuth(this, options.auth) : null;
        this._dspLocalStorage = options.dspLocalStorage;
        this._dspSessionStorage = options.dspSessionStorage;
        this._homePageUri = Utility.checkUndefined(options.homePageUri, Utility.isHtmlHost ? window.location.origin : null);
        this.isAutoReload = Utility.checkUndefined<boolean>(options.isAutoReload, true);
        this.isLogEnabled = Utility.checkUndefined<boolean>(options.isLogEnabled, true);
        this.isUseAppErrorHandler = Utility.checkUndefined<boolean>(options.isUseAppErrorHandler, true);
        this._originalUri = Utility.isHtmlHost ? window.location.href : null;
        this._originalQueryString = Utility.isHtmlHost ? window.location.search : null;
        this._ajaxProvider = options.ajaxProvider ? options.ajaxProvider : new DirectSpXmlHttpAjaxProvider();
    }

    public get homePageUri(): string | null { return this._homePageUri; }
    public get storageNamePrefix(): string { return this._storageNamePrefix; }
    public get ajaxProvider(): IDirectSpAjaxProvider { return this._ajaxProvider };
    public get resourceApiUri(): string { return this._resourceApiUri; }
    public get auth(): DirectSpAuth | null { return this._auth; }
    public get originalQueryString(): string | null { return this._originalQueryString; }
    public get originalUri(): string | null { return this._originalUri; }
    public get resourceAppVersion(): string | null { return this._resourceAppVersion; }
    public get dspLocalStorage(): IDirectSpStorage { return this._dspLocalStorage; }
    public get dspSessionStorage(): IDirectSpStorage { return this._dspSessionStorage; }
    public get sessionState(): string { return this._sessionState; }

    //navigate to directSp auth server
    public init(): Promise<boolean> {
        console.log("DirectSp: initializing ...");
        return this._load().then(() => {
            return this._auth ? this._auth.init() : true;

        });
    }

    private _load(): Promise<void> {
        let promises = [];
        let prefix: string = this._storageNamePrefix;

        try {
            let promise = null;

            //load resourceAppVersion
            promise = this.dspLocalStorage.getItem(prefix + "resouceAppVersion").then(data => {
                this._resourceAppVersion = data;
            });
            promises.push(promise);

            //load sessionState; use current session if there is not session
            promise = this.dspSessionStorage.getItem(prefix + "sessionState").then(data => {
                if (data)
                    this._sessionState = data;
                else
                    this.dspSessionStorage.setItem(prefix + "sessionState", this._sessionState);

            });
            promises.push(promise);

        } catch (err) { }

        return Promise.all(promises)
            .then(() => { });
    };

    /**
     * 
     * @param error Will be converted to DirectSpError
     * @param isUseAppErrorHandler  
     *  default use global isUseAppErrorHandler
     *  true: use global handler
     *  false just convert and throw the error
     */
    public throwAppError(error: any, isUseAppErrorHandler?: boolean): void {
        error = DirectSpError.create(error);
        isUseAppErrorHandler = Convert.toBoolean(isUseAppErrorHandler, this.isUseAppErrorHandler);

        if (!this.onError || !isUseAppErrorHandler)
            throw error;

        // create error controller
        const errorController: DirectSpErrorController = new DirectSpErrorController({ error: error, dspClient: this });
        this.onError(errorController);
    };

    public invokeBatch(spCalls: IDirectSpCall[], invokeOptions?: IDirectSpInvokeOptions): Promise<IDirectSpInvokeResult[]> {
        let invokeParams: IDirectSpInvokeParams = {
            spCalls: spCalls,
            invokeOptions: invokeOptions
        };

        return this._invokeCore(invokeParams);
    };

    //invokeOptions {seqGroup:"SequenceGroupName ", pageSize:10, pageIndex:0}
    public invoke(method: string, params?: any, invokeOptions?: IDirectSpInvokeOptions): Promise<IDirectSpInvokeResult> {

        const invokeParams: IDirectSpInvokeParams = {
            spCall: {
                method: method,
                params: params
            },
            invokeOptions: invokeOptions
        };

        return this.invoke2(invokeParams);
    };

    public invoke2(invokeParams: IDirectSpInvokeParams): Promise<IDirectSpInvokeResult> {
        //validate
        if (!invokeParams.spCall) throw new DirectSpError("spCall is expected");
        if (!invokeParams.spCall.method) throw new DirectSpError("spCall.method is expected");
        if (!this.resourceApiUri) throw new DirectSpError("resourceApiUri has not been set!");

        //call api
        return this._invokeCore(invokeParams).then(result => {
            // manage auto download
            if (invokeParams.invokeOptions && invokeParams.invokeOptions.autoDownload) {
                if (!Utility.isHtmlHost)
                    throw new exceptions.NotSupportedException("autoDownload");
                window.location = result.recordsetUri;
            }
            return result;
        });
    };


    // Invoke Preperation
    // manage seqGroup, write log and append requestId
    private _invokeCore(invokeParams: IDirectSpInvokeParams): Promise<any> {

        //set default options
        if (!invokeParams.invokeOptions) invokeParams.invokeOptions = {};
        if (invokeParams.invokeOptions.autoDownload == true) {
            invokeParams.invokeOptions.isWithRecodsetDownloadUri = true;
            if (!invokeParams.invokeOptions.recordCount) invokeParams.invokeOptions.recordCount = -2;
            if (!invokeParams.invokeOptions.recordsetFormat) invokeParams.invokeOptions.recordsetFormat = "tabSeparatedValues";
        }

        //set defaults
        if (!invokeParams.invokeOptions) invokeParams.invokeOptions = {};
        invokeParams.invokeOptions.cache == Convert.toBoolean(invokeParams.invokeOptions.cache, true);
        invokeParams.invokeOptions.isUseAppErrorHandler = Convert.toBoolean(invokeParams.invokeOptions.isUseAppErrorHandler, this.isUseAppErrorHandler);

        //log request
        const method: string = invokeParams.spCall ? invokeParams.spCall.method : "invokeBatch";
        if (this.isLogEnabled)
            console.log("DirectSp: invoke (Request)", method, invokeParams);

        // check seqGroup
        let seqGroup: string | null = invokeParams.invokeOptions.seqGroup ? invokeParams.invokeOptions.seqGroup : null;
        let seqGroupValue: number | null | undefined;
        if (seqGroup) {
            seqGroupValue = this._seqGroups[seqGroup]
                ? this._seqGroups[seqGroup] + 1
                : 1;
            this._seqGroups[seqGroup] = seqGroupValue;
        }

        // append request id to invoke options
        invokeParams.invokeOptions.requestId = Utility.generateGuid();

        //invoke
        return this._invokeCore2(invokeParams)
            .then(result => {
                if (seqGroup && seqGroupValue && seqGroupValue != this._seqGroups[seqGroup])
                    throw new DirectSpError("request has been suppressed by seqGroup!");

                //log response
                if (this.isLogEnabled)
                    console.log("DirectSp: invoke (Response)", method, invokeParams, result);

                return result;
            })
            .catch(error => {
                if (seqGroup && seqGroupValue && seqGroupValue != this._seqGroups[seqGroup])
                    throw new DirectSpError("request has been suppressed by seqGroup!");

                if (this.isLogEnabled)
                    console.warn("DirectSp: invoke (Response)", method, invokeParams, error);
                throw error;
            });
    };

    //Handle Hook and delay
    private async _invokeCore2(invokeParams: IDirectSpInvokeParams): Promise<any> {
        // manage hook
        let hookParams: IDirectSpHookParams = {
            invokeParams: invokeParams,
            delay: 0,
            isHandled: false
        };

        let result = await this._processInvokeHook(hookParams);
        if (!hookParams.isHandled)
            result = this._invokeCore3(hookParams.invokeParams);

        //return the promise if there is no api delay
        if (hookParams.delay == null || hookParams.delay <= 0)
            return result;

        //proces delay
        return new Promise((resolve) => {
            let interval = hookParams.delay;
            let delay = Utility.getRandomInt(interval / 2, interval + interval / 2);
            console.warn(`DirectSp: Warning! {method} is delayed by {delay} milliseconds`);
            setTimeout(() => resolve(), delay);
        });
    };

    // Convert to fetch
    private async _invokeCore3(invokeParams: IDirectSpInvokeParams): Promise<any> {

        const method: string = invokeParams.spCall ? invokeParams.spCall.method : "invokeBatch";
        const result = await this._fetch({
            url: Uri.combine(this.resourceApiUri, method),
            data: invokeParams,
            method: "POST",
            headers: {
                authorization: this.auth ? this.auth.authorizationHeader : null,
                "Content-Type": "application/json;charset=UTF-8"
            },
            cache: invokeParams.invokeOptions ? invokeParams.invokeOptions.cache : false
        });

        this._checkNewVersion(result.headers["DSP-AppVersion"]);
        return JSON.parse(result.data);
    };

    //manage onError
    public async _fetch(request: IDirectSpRequest): Promise<IDirectSpResponse> {
        try {
            return this._fetch2(request);
        }
        catch (error) {
            // create error controller
            let errorController = new DirectSpErrorController({ error: error, dspClient: this, request: request });
            let invokeOptions = request.data && request.data.invokeOptions ? <IDirectSpInvokeOptions>request.data.invokeOptions : null;
            let isUseAppHandler = invokeOptions && invokeOptions.isUseAppErrorHandler;

            //we should call onError if the exception can be retried
            if (this.onError && (errorController.canRetry || isUseAppHandler)) {
                if (this.isLogEnabled)
                    console.log("DirectSp: Calling onError ...");
                this.onError(errorController);
                return errorController.promise;
            }
            throw error;
        }
    }

    //manage RefreshToken
    private async _fetch2(request: IDirectSpRequest): Promise<IDirectSpResponse> {

        //check if request require authentication
        if (!this.auth || !request.headers || !request.headers.authorization)
            return this._fetchProvider(request); //there is no token

        //refreshing token
        if (request.data && request.data.grant_type == 'refresh_token')
            return this._fetchProvider(request); //prevent loop

        await this.auth.refreshToken();
        request.headers.authorization = this.auth.authorizationHeader; //update request token

        // fetch again with valid token
        return this._fetch(request);
    }

    private _fetchProvider(request: IDirectSpRequest): Promise<IDirectSpResponse> {
        return this._ajaxProvider.fetch(request);
    };

    /**
     * @param resourceAppVersion version which is retrieved from the server
     * @returns true current version is different from server version
     */
    private _checkNewVersion(resourceAppVersion: string): boolean {
        // app versin does not available if resourceAppVersion is null
        if (!resourceAppVersion || resourceAppVersion == this.resourceAppVersion)
            return false;

        //detect new versio
        const isReloadNeeded: boolean =
            this.resourceAppVersion != null &&
            this.resourceAppVersion != resourceAppVersion;

        // save new version
        this._resourceAppVersion = resourceAppVersion;
        this.dspLocalStorage.setItem(
            this._storageNamePrefix + "resouceAppVersion",
            resourceAppVersion
        );

        // reloading
        if (isReloadNeeded) {

            //call new version event
            if (this.onNewVersion)
                this.onNewVersion();

            // auto reload page
            if (this.isAutoReload && Utility.isHtmlHost) {
                console.log("DirectSp: New version detected! Reloading ...");
                window.location.reload(true);
            }
        }

        return isReloadNeeded;
    };

    private async _processInvokeHook(hookParams: IDirectSpHookParams): Promise<IDirectSpInvokeResult> {
        //return quickly if there is no hook
        if (!this.onBeforeInvoke)
            return {};

        // batch does not supported
        if (!hookParams.invokeParams.spCall || hookParams.invokeParams.spCall.method == 'invokeBatch')
            return {};

        //run hook
        try {

            let result = await this.onBeforeInvoke(hookParams);
            if (hookParams.isHandled)
                //log hook
                if (this.isLogEnabled)
                    console.warn("DirectSp: Hooking > ", hookParams.invokeParams.spCall.method, hookParams, result);
            return Utility.clone(result);

        } catch (e) {
            // convert user errors to DirectSpError
            throw DirectSpError.create(e);
        }
    };

    public async help(criteria: string | null = null, reload: boolean = false): Promise<string> {

        //Load Api info if it is not loaded
        if (!this._systemApi || reload) {
            let result: IDirectSpInvokeResult = await this.invoke("System_Api");
            this._systemApi = result.api;
            if (!result.api)
                return "DirectSp: Could not retreive api information!";
        }

        return DirectSpHelp.help(this._systemApi, criteria);
    };

};
