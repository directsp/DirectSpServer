declare module "DirectSpUtil" {
    export interface IDirectSpKeyToAny {
        [ket: string]: any;
    }
    export class Convert {
        static toBoolean(value: string | number | undefined | null | boolean, defaultValue?: boolean): boolean;
        static toInteger(value: any, defaultValue?: number): number;
        static toQueryString(obj: any): string;
        static buffer_fromBase64(base64String: string): string;
    }
    export class Html {
        static submit(url: string, params: any): void;
    }
    export class Uri {
        static getParameterByName: (name: string, url?: string | undefined) => string | null;
        static combine: (uriBase: string, uriRelative: string) => string;
        static getParent: (uri: string) => string;
        static getUrlWithoutQueryString(uri: string): string;
        static getFileName(uri: string): string | null;
    }
    export class Utility {
        static checkUndefined<T>(value: T | undefined, defValue: T): T;
        static parseJwt(token: string): any;
        static clone<T>(obj: T): T;
        static getRandomInt(min: number, max: number): number;
        static generateGuid(): string;
        static toCamelcase(str: string): string;
        static tryParseJason(json: string): any;
        static readonly isHtmlHost: boolean;
    }
}
declare module "DirectSpError" {
    export class DirectSpError extends Error {
        errorType: string | null;
        errorName: string | null;
        errorNumber: number | null;
        errorMessage: string | null;
        errorDescription: string | null;
        errorProcName: string | null;
        status: number | null;
        statusText: string | null;
        innerError: Error | null;
        errorData: any;
        constructor(message: string);
        static create(data: any): DirectSpError;
    }
    export namespace exceptions {
        class NotSupportedException extends DirectSpError {
            constructor(message?: string);
        }
        class NotImplementedException extends DirectSpError {
            constructor(message?: string);
        }
    }
}
declare module "DirectSpAjax" {
    import { IDirectSpKeyToAny } from "DirectSpUtil";
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
        fetch(request: IDirectSpRequest): Promise<IDirectSpResponse>;
    }
}
declare module "DirectSpStorage" {
    export interface IDirectSpStorage {
        getItem(key: string): Promise<string | null>;
        setItem(key: string, value: string): Promise<void>;
        removeItem(key: string): Promise<void>;
    }
    export class DirectSpHtmlStorage implements IDirectSpStorage {
        private _storage;
        constructor(storage: Storage);
        getItem(key: string): Promise<string | null>;
        setItem(key: string, value: string): Promise<void>;
        removeItem(key: string): Promise<void>;
    }
}
declare module "DirectSpErrorController" {
    import { DirectSpError } from "DirectSpError";
    import { IDirectSpRequest, IDirectSpResponse } from "DirectSpAjax";
    import { DirectSpClient } from "DirectSpClient";
    export interface IDirectSpErrorControllerOptions {
        error: DirectSpError;
        dspClient: DirectSpClient;
        request?: IDirectSpRequest;
    }
    export class DirectSpErrorController {
        captchaCode: string | null;
        private _data;
        private _error;
        private _errorNumber;
        private readonly _canRetry;
        private readonly _captchaImageUri;
        private readonly _promise;
        private _reject;
        private _resolve;
        constructor(data: IDirectSpErrorControllerOptions);
        readonly promise: Promise<IDirectSpResponse>;
        readonly error: DirectSpError;
        readonly captchaImageUri: string | null;
        readonly canRetry: boolean;
        readonly dspClient: DirectSpClient;
        retry(): void;
        release(): void;
    }
}
declare module "DirectSpHelp" {
    export class DirectSpHelp {
        static help(systemApi: any, criteria?: string | null): string;
        private static _helpImpl;
        private static _getHelpParam;
        private static _getHelpParamType;
        private static _formatHelpParamName;
        private static _formatHelpParam;
    }
}
declare module "DirectSpClient" {
    import { IDirectSpRequest, IDirectSpResponse, IDirectSpAjaxProvider } from "DirectSpAjax";
    import { DirectSpAuth, IDirectSpAuthOptions, IDirectSpAuthorizedData } from "DirectSpAuth";
    import { IDirectSpStorage } from "DirectSpStorage";
    import { IDirectSpKeyToAny } from "DirectSpUtil";
    import { DirectSpErrorController } from "DirectSpErrorController";
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
        delay: number;
    }
    /**
     * @see: https://github.com/directsp
     * @event onAuthorized: check the authorization state
     * @event onError: fire when an herror occurred
     * @event: onNewVersion: fire when new API version detected
     * @event: onBeforeInvoke: fire beofore any invoke
     */
    export class DirectSpClient {
        isAutoReload: boolean;
        isLogEnabled: boolean;
        isUseAppErrorHandler: boolean;
        private readonly _homePageUri;
        private readonly _storageNamePrefix;
        private readonly _resourceApiUri;
        private readonly _ajaxProvider;
        private readonly _auth;
        private readonly _originalUri;
        private readonly _originalQueryString;
        private readonly _dspSessionStorage;
        private readonly _dspLocalStorage;
        private readonly _seqGroups;
        private _sessionState;
        private _resourceAppVersion;
        private _systemApi;
        onError: ((error: DirectSpErrorController) => void) | null;
        onAuthorized: ((data: IDirectSpAuthorizedData) => void) | null;
        onNewVersion: (() => void) | null;
        onBeforeInvoke: ((hookParams: IDirectSpHookParams) => Promise<IDirectSpRequest>) | null;
        constructor(options: IDirectSpOptions);
        readonly homePageUri: string | null;
        readonly storageNamePrefix: string;
        readonly ajaxProvider: IDirectSpAjaxProvider;
        readonly resourceApiUri: string;
        readonly auth: DirectSpAuth | null;
        readonly originalQueryString: string | null;
        readonly originalUri: string | null;
        readonly resourceAppVersion: string | null;
        readonly dspLocalStorage: IDirectSpStorage;
        readonly dspSessionStorage: IDirectSpStorage;
        readonly sessionState: string;
        init(): Promise<boolean>;
        private _load;
        /**
         *
         * @param error Will be converted to DirectSpError
         * @param isUseAppErrorHandler
         *  default use global isUseAppErrorHandler
         *  true: use global handler
         *  false just convert and throw the error
         */
        throwAppError(error: any, isUseAppErrorHandler?: boolean): void;
        invokeBatch(spCalls: IDirectSpCall[], invokeOptions?: IDirectSpInvokeOptions): Promise<IDirectSpInvokeResult[]>;
        invoke(method: string, params?: any, invokeOptions?: IDirectSpInvokeOptions): Promise<IDirectSpInvokeResult>;
        invoke2(invokeParams: IDirectSpInvokeParams): Promise<IDirectSpInvokeResult>;
        private _invokeCore;
        private _invokeCore2;
        private _invokeCore3;
        _fetch(request: IDirectSpRequest): Promise<IDirectSpResponse>;
        private _fetch2;
        private _fetchProvider;
        /**
         * @param resourceAppVersion version which is retrieved from the server
         * @returns true current version is different from server version
         */
        private _checkNewVersion;
        private _processInvokeHook;
        help(criteria?: string | null, reload?: boolean): Promise<string>;
    }
}
declare module "DirectSpAuth" {
    import { DirectSpClient } from "DirectSpClient";
    import { DirectSpError } from "DirectSpError";
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
    export class DirectSpAuth {
        isAutoSignIn: boolean;
        readonly _dspClient: DirectSpClient;
        private readonly _clientId;
        private readonly _redirectUri;
        private readonly _baseEndpointUri;
        private readonly _authorizeEndpointUri;
        private readonly _tokenEndpointUri;
        private readonly _userinfoEndpointUri;
        private readonly _logoutEndpointUri;
        private readonly _scope;
        private readonly _type;
        private readonly _refreshClockSkew;
        private _authRequest;
        private _userInfo;
        private _isPersistent;
        private _tokens;
        private _authError;
        private _accessTokenInfo;
        private _lastPageUri;
        constructor(dspClient: DirectSpClient, options: IDirectSpAuthOptions);
        readonly clientId: string;
        readonly redirectUri: string | null;
        readonly authorizeEndpointUri: string | null;
        readonly tokenEndpointUri: string | null;
        readonly userinfoEndpointUri: string | null;
        readonly logoutEndpointUri: string | null;
        readonly type: string | null;
        readonly scope: string;
        readonly tokens: IToken | null;
        readonly authError: DirectSpError | null;
        readonly accessTokenInfo: IAccessTokenInfo | null;
        readonly isAuthorized: boolean;
        readonly userId: string | null;
        readonly userInfo: IUserInfo | null;
        readonly dspClient: DirectSpClient;
        private readonly storageNamePrefix;
        isPersistent: boolean;
        private _setUserInfo;
        readonly authorizationHeader: string | null;
        readonly username: string | null;
        readonly userDisplayName: string | null;
        readonly baseEndpointUri: string | null;
        readonly authRequest: IAuthRequest | null;
        readonly authRequestUri: string | null;
        readonly isAuthRequest: string | null;
        readonly isAuthCallback: boolean | "" | null;
        init(): Promise<boolean>;
        private _load;
        private _processAuthCallback;
        setTokens(value: IToken | null, refreshUserInfo?: boolean): Promise<boolean>;
        private _refreshUserInfo;
        private _getUserInfo;
        private _isTokenExpiredError;
        refreshToken(): Promise<void>;
        private _resetUser;
        private _fireAuthorizedEvent;
        signInByPasswordGrant(username: string, password: string): Promise<IToken>;
        signOut(clearUser?: boolean, redirect?: boolean): void;
        grantAuthorization(password: string): void;
        denyAuthorization(): void;
        signIn(): void;
    }
}
declare module "directSp" {
    export * from "DirectSpAjax";
    export * from "DirectSpStorage";
    export * from "DirectSpError";
    export * from "DirectSpClient";
    export * from "DirectSpUtil";
}
