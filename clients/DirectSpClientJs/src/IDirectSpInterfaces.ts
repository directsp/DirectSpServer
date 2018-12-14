// just to remove circular dependencies

//General
export interface IDirectSpKeyToAny {
    [ket: string]: any
}

//Platform
export interface IDirectSpControl {
    readonly location: URL | null;
    reload(): Promise<void>;
    navigate(uri: string): Promise<void>;
    download(uri: string): Promise<void>;
}

// Ajax
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
    headers?: any;
}

export interface IDirectSpAjaxProvider {
    fetch(request: IDirectSpRequest): Promise<IDirectSpResponse>;
}

// Authentication
export interface IAuthRequest {
    client_id: string;
    redirect_uri: string | null;
    scope: string | null;
    response_type: string | null;
    state: string | null;
}

export interface IUserInfo {
    username: string | null;
    name: string | null;
}

export interface IToken {
    access_token: string;
    expires_in: number;
    refresh_token: string | null;
    token_type: string;
}

export interface IAccessTokenInfo {
    sub: string;
    exp: string;
}

export interface IDirectSpAuthorizedData {
    lastPageUri: string | null;
}

export interface IDirectSpAuthOptions {
    clientId?: string;
    redirectUri?: string | null;
    baseEndpointUri?: string | null;
    authorizeEndpointUri?: string | null;
    tokenEndpointUri?: string | null;
    userinfoEndpointUri?: string | null;
    logoutEndpointUri?: string | null;
    scope?: string;
    type?: string;
    isPersistent?: boolean;
    isAutoSignIn?: boolean;
}

//Storage
export interface IDirectSpStorage {
    getItem(key: string): Promise<string | null>;
    setItem(key: string, value: string): Promise<void>;
    removeItem(key: string): Promise<void>;
}

//Client
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
    control?: IDirectSpControl;
    sessionState?: string;
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
    invokeParams: IDirectSpInvokeParams;
    delay: number
    isRandomDelay: boolean;
}

export interface IDirectSpClient
{
    control: IDirectSpControl;
    isLogEnabled: boolean;
    onAuthorized: ((data: IDirectSpAuthorizedData) => void) | null;
    sessionState: string | null;
    homePageUri: string | null;    
    originalUri: URL | null;
    storageNamePrefix: string;
    dspSessionStorage: IDirectSpStorage;
    dspLocalStorage: IDirectSpStorage;

    _fetch(request: IDirectSpRequest): Promise<IDirectSpResponse>;
}