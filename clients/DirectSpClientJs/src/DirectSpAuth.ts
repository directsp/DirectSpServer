import { DirectSpClient } from "./DirectSpClient";
import { DirectSpError, exceptions } from "./DirectSpError";
import { Utility, Uri, Convert, Html } from "./DirectSpUtil";
import { IDirectSpStorage } from "./DirectSpStorage";
import { IDirectSpRequest } from "./DirectSpAjax";

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
    public isAutoSignIn: boolean = false;
    public readonly _dspClient: DirectSpClient;
    private readonly _clientId: string;
    private readonly _redirectUri: string | null = null;
    private readonly _baseEndpointUri: string | null = null;
    private readonly _authorizeEndpointUri: string | null = null;
    private readonly _tokenEndpointUri: string | null = null;
    private readonly _userinfoEndpointUri: string | null = null;
    private readonly _logoutEndpointUri: string | null = null;
    private readonly _scope: string = "offline_access";
    private readonly _type: string = "token";
    private readonly _refreshClockSkew: number = 60;
    private _authRequest: IAuthRequest | null = null;
    private _userInfo: IUserInfo | null = null;
    private _isPersistent: boolean = false;
    private _tokens: IToken | null = null;
    private _authError: DirectSpError | null = null;
    private _accessTokenInfo: IAccessTokenInfo | null = null
    private _lastPageUri: string | null = null;

    public constructor(dspClient: DirectSpClient, options: IDirectSpAuthOptions) {
        this._dspClient = dspClient;

        this._lastPageUri = dspClient.homePageUri;
        this._clientId = Utility.checkUndefined(options.clientId, "");
        this._redirectUri = Utility.checkUndefined(options.redirectUri, dspClient.originalUri ? dspClient.originalUri.origin + "/oauth2/callback" : null);
        this._scope = Utility.checkUndefined(options.scope, this._scope); //openid offline_access profile phone email address
        this._type = Utility.checkUndefined(options.type, this._type); //token, code
        this._isPersistent = Utility.checkUndefined(options.isPersistent, this._isPersistent);
        this.isAutoSignIn = Utility.checkUndefined(options.isAutoSignIn, this.isAutoSignIn);

        // must set before other endpoint to prevent override
        if (options.baseEndpointUri) {
            this._baseEndpointUri = options.baseEndpointUri;
            this._authorizeEndpointUri = Uri.combine(options.baseEndpointUri, "/connect/authorize");
            this._tokenEndpointUri = Uri.combine(options.baseEndpointUri, "/connect/token");
            this._userinfoEndpointUri = Uri.combine(options.baseEndpointUri, "/connect/userinfo");
            this._logoutEndpointUri = Uri.combine(options.baseEndpointUri, "/connect/logout");
        }

        if (options.authorizeEndpointUri) this._authorizeEndpointUri = options.authorizeEndpointUri;
        if (options.tokenEndpointUri) this._tokenEndpointUri = options.tokenEndpointUri;
        if (options.userinfoEndpointUri) this._userinfoEndpointUri = options.userinfoEndpointUri;
        if (options.logoutEndpointUri) this._logoutEndpointUri = options.logoutEndpointUri;

        const requestClientId: string | null = this._getParameterByName("client_id");
        if (requestClientId) {
            this._authRequest =
                {
                    client_id: requestClientId,
                    redirect_uri: this._getParameterByName("redirect_uri"),
                    scope: this._getParameterByName("scope"),
                    response_type: this._getParameterByName("response_type"),
                    state: this._getParameterByName("state")
                };
        }


        this._accessTokenInfo = null;
    }

    public get clientId(): string { return this._clientId; }
    public get redirectUri(): string | null { return this._redirectUri; }
    public get authorizeEndpointUri(): string | null { return this._authorizeEndpointUri; }
    public get tokenEndpointUri(): string | null { return this._tokenEndpointUri; }
    public get userinfoEndpointUri(): string | null { return this._userinfoEndpointUri; }
    public get logoutEndpointUri(): string | null { return this._logoutEndpointUri; }
    public get type(): string | null { return this._type; }
    public get scope(): string { return this._scope; }
    public get tokens(): IToken | null { return this._tokens; }
    public get authError() { return this._authError; }
    public get accessTokenInfo() { return this._accessTokenInfo; }
    public get isAuthorized(): boolean { return this._tokens != null; }
    public get userId(): string | null { return this.accessTokenInfo ? this.accessTokenInfo.sub : null; }
    public get userInfo(): IUserInfo | null { return this._userInfo; }
    public get dspClient(): DirectSpClient { return this._dspClient; }
    private get storageNamePrefix(): string { return this._dspClient.storageNamePrefix; }

    public get isPersistent(): boolean { return this._isPersistent; }
    public set isPersistent(value: boolean) {
        this._isPersistent = value;
        this.dspClient.dspLocalStorage.setItem(this.storageNamePrefix + "isPersistent", value.toString());
    }

    private _getParameterByName(paramName: string): string | null {
        if (!this.dspClient.originalUri)
            return null;
        return Uri.getParameterByName(paramName, this.dspClient.originalUri.href);
    }

    private _setUserInfo(value: IUserInfo | null) {
        this._userInfo = value;
        if (value)
            this.dspClient.dspLocalStorage.setItem(this.storageNamePrefix + "userInfo", JSON.stringify(value));
        else
            this.dspClient.dspLocalStorage.removeItem(this.storageNamePrefix + "userInfo");
    }

    public get authorizationHeader(): string | null {
        return this.tokens
            ? this.tokens.token_type + " " + this.tokens.access_token
            : null;
    }

    public get username(): string | null {
        return this.userInfo ? this.userInfo.username : null;
    }

    public get userDisplayName(): string | null {
        if (!this.userInfo)
            return null;
        return this.userInfo.name ? this.userInfo.name : this.userInfo.username;
    }

    public get baseEndpointUri(): string | null {
        return this._baseEndpointUri;
    }

    public get authRequest(): IAuthRequest | null { return this._authRequest; }
    public get authRequestUri(): string | null {
        if (!this.dspClient.originalUri)
            return null;
        return this.dspClient.originalUri.origin + "?" + Convert.toQueryString(this._authRequest);
    }

    public get isAuthRequest(): boolean {
        return this.authRequest != null && this.authRequest.client_id != null && this.authRequest.state != null;
    }

    public get isAuthCallback(): boolean {
        let callbackPattern = this.redirectUri;
        return callbackPattern != null && this.dspClient.originalUri != null && this.dspClient.originalUri.href.indexOf(callbackPattern) != -1;
    }

    public async init(): Promise<boolean> {
        //process error
        if (this._getParameterByName("error") != null) {
            this._authError = DirectSpError.create({
                error: this._getParameterByName("error"),
                error_description: this._getParameterByName("error_description")
            });

            console.error("DirectSp: Auth Error!", this._authError);
            if (this.isAutoSignIn) {
                this.isAutoSignIn = false;
                console.warn("DirectSp: isAutoSignIn is set to false due Auth Error", this.authError);
            }
        }

        try {
            await this._load();
            await this.refreshToken();
            await this._processAuthCallback();
            this._fireAuthorizedEvent();
            return true;
        }
        catch (error) {
            this._fireAuthorizedEvent();
            throw error;
        }

    }

    private async _load(): Promise<void> {
        let promises = [];
        let prefix: string = this.storageNamePrefix;

        const localStorage: IDirectSpStorage = this.dspClient.dspLocalStorage;
        const sessionStorage: IDirectSpStorage = this.dspClient.dspLocalStorage;

        try {
            let promise: Promise<any>;

            //tokens
            promise = sessionStorage.getItem(prefix + "auth.tokens").then(data => {
                if (!data)
                    return localStorage.getItem(prefix + "auth.tokens");
                return data;
            }).then(data => {
                if (data) this._tokens = JSON.parse(data);
            });
            promises.push(promise);

            //userInfo
            promise = sessionStorage.getItem(prefix + "userInfo").then(data => {
                if (!data)
                    return localStorage.getItem(prefix + "userInfo");
                return data;
            }).then(data => {
                if (data) this._userInfo = JSON.parse(data);
            });

            //isPersistent
            promise = localStorage.getItem(prefix + "isPersistent").then(data => {
                if (data != null)
                    this._isPersistent = Convert.toBoolean(data);
            });
            promises.push(promise);

            promise = this.dspClient.dspSessionStorage.getItem(prefix + "lastPageUri").then(data => {
                if (data)
                    this._lastPageUri = data;
            });
            promises.push(promise);

        } catch (err) { }


        await Promise.all(promises);
    };

    //navigate to directSp authorization server
    private async _processAuthCallback(): Promise<boolean> {

        //check is oauth2callback
        if (!this.isAuthCallback)
            return false;

        //check state and do nothing if it is not matched
        let state = this._getParameterByName("state");
        if (this.dspClient.sessionState != state) {
            this.setTokens(null);
            throw new DirectSpError("Invalid sessionState!");
        }

        //process authorization_code flow
        const code = this._getParameterByName("code");
        if (code != null) {

            //validating configuration
            if (!this.authorizeEndpointUri) throw new DirectSpError("authorizeEndpointUri has not been set!");
            if (!this.tokenEndpointUri) throw new DirectSpError("tokenEndpointUri has not been set!");
            if (!this.redirectUri) throw new DirectSpError("redirectUri has not been set!");

            const data = {
                client_id: this.clientId,
                redirect_uri: this.redirectUri,
                grant_type: "authorization_code",
                code: code
            };

            try {
                const response = await this.dspClient._fetch({
                    url: this.tokenEndpointUri,
                    headers: { "Content-Type": "application/x-www-form-urlencoded;charset=utf-8" },
                    data: data,
                    method: "POST"
                });
                const tokens: IToken = JSON.parse(response.data);
                return await this.setTokens(tokens);
            }
            catch (error) {
                this.setTokens(null);
                throw error;
            }
        }

        //process implicit flow
        const access_token: string | null = this._getParameterByName("access_token");
        if (access_token) {

            //make sure token_type exists
            const token_type: string | null = this._getParameterByName("token_type");
            if (!token_type)
                throw new DirectSpError("access_Token returned without token_type!");

            // set new token
            await this.setTokens({
                access_token: access_token,
                token_type: token_type,
                expires_in: Convert.toInteger(this._getParameterByName("expires_in"), 0),
                refresh_token: null,
            });
        }

        //finish processAuthCallback without any result
        return false;
    };

    public async setTokens(value: IToken | null, refreshUserInfo: boolean = true): Promise<boolean> {

        // check is token changed
        if (value == this._tokens)
            return true; //no change

        //set token
        this._tokens = value;

        //update auth data
        if (value != null) {
            //clear token if it was invalid
            try {
                this._accessTokenInfo = Utility.parseJwt(value.access_token);
            } catch (error) {
                return this.setTokens(null);
            }

            //refreshing UserInfo
            if (refreshUserInfo)
                await this._refreshUserInfo();
        }

        // save token
        const prefix: string = this.dspClient.storageNamePrefix;
        const tokenString = JSON.stringify(this._tokens);

        //save tokens
        this.dspClient.dspSessionStorage.setItem(prefix + "auth.tokens", tokenString);
        if (this.isPersistent)
            this.dspClient.dspLocalStorage.setItem(prefix + "auth.tokens", tokenString);
        else
            this.dspClient.dspLocalStorage.removeItem(prefix + "auth.tokens");

        // return the token
        return value != null;
    };

    private async _refreshUserInfo(): Promise<boolean> {
        try {
            this._userInfo = await this._getUserInfo();
            this._setUserInfo(this._userInfo);
            return true;
        }
        catch (e) {
            console.error("Failed to retrieve user info!", e);
            return false;
        }
    };

    private async _getUserInfo(): Promise<IUserInfo> {
        if (!this.isAuthorized) {
            let error: DirectSpError = new DirectSpError("Can not refresh token for unauthorized users");
            error.errorName = "unauthorized";
            throw error;
        }

        if (!this.userinfoEndpointUri) {
            let error: DirectSpError = new DirectSpError("userinfoEndpointUri has not been set!");
            error.errorName = "bad_configuration";
            throw error;
        }

        const request: IDirectSpRequest = {
            url: this.userinfoEndpointUri,
            method: "GET",
            headers: { authorization: this.authorizationHeader }
        };

        // fetch data
        const result = await this.dspClient._fetch(request);
        const userInfo: IUserInfo = JSON.parse(result.data);
        if (this.dspClient.isLogEnabled)
            console.log("DirectSp: userInfo", userInfo);
        return userInfo;
    };

    private _isTokenExpiredError(error: DirectSpError): boolean {
        if (!error)
            return false;

        //check database AccessDeniedOrObjectNotExists error; it means token has been validated
        if (error.errorName == "AccessDeniedOrObjectNotExists")
            return false;

        //noinspection JSUnresolvedVariable
        if (error.errorName == "invalid_grant")
            return true;

        return error.status == 401;
    };


    // return false if the current token is not valid
    public async refreshToken(): Promise<void> {

        //check expiration time
        if (this.accessTokenInfo && this.accessTokenInfo["exp"]) {
            let dateNow = new Date();
            if (parseInt(this.accessTokenInfo["exp"]) - this._refreshClockSkew > dateNow.getTime() / 1000)
                return; //token is not refreshed because it is not required
        }

        //return if token not exists
        if (!this.tokens || !this.tokens.refresh_token || !this.accessTokenInfo) {

            //current token is not valid and can not be refreshed
            if (this.isAuthorized)
                this.signOut(false, this.isAutoSignIn);
            return;
        }

        //Refreshing token
        console.log("DirectSp: Refreshing current token ...");
        if (!this.tokenEndpointUri)
            throw new DirectSpError("tokenEndpointUri has not been set for refreshing token!");

        //call
        const request: IDirectSpRequest = {
            url: this.tokenEndpointUri,
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded;charset=utf-8",
                authorization: this.authorizationHeader
            },
            data: {
                grant_type: "refresh_token",
                client_id: this.clientId,
                refresh_token: this.tokens.refresh_token,
            }
        };

        try {
            //current token has been refreshed and valid
            const result = await this.dspClient._fetch(request);
            await this.setTokens(JSON.parse(result.data), false);
        }
        catch (error) {
            if (this.isAuthorized && this._isTokenExpiredError(error))
                this.signOut(false, this.isAutoSignIn);
            throw error;
        }
    };

    private _resetUser(): void {
        this.setTokens(null);
        this._setUserInfo(null);
    };


    private _fireAuthorizedEvent(): void {
        setTimeout(() => {

            //fire the event
            if (this.dspClient.isLogEnabled) {
                if (this.isAuthorized)
                    console.log("DirectSp: User has been authorized", this.userInfo);
                else
                    console.log("DirectSp: User has not been authorized!");
            }

            //auto signin
            if (!this.isAuthorized && this.isAutoSignIn) {
                this.signIn();
                return;
            }

            //trigger the event
            if (this.dspClient.onAuthorized) {
                let data = { lastPageUri: this._lastPageUri };
                this.dspClient.onAuthorized(data);
            }

            this._lastPageUri = null; //reset last page notifying user
            this.dspClient.dspSessionStorage.removeItem(this.storageNamePrefix + "lastPageUri");
        }, 0);
    };

    public async signInByPasswordGrant(username: string, password: string): Promise<IToken> {

        if (!this.tokenEndpointUri)
            throw new DirectSpError("tokenEndpointUri has not been set!");

        //saving current state
        const orgIsAuthorized = this.isAuthorized;

        //clear user info and tokens
        if (this.username != username) {
            this._resetUser();
        }

        //create request param
        let requestParam = {
            username: username,
            password: password,
            grant_type: "password",
            scope: this.scope,
            client_id: this.clientId
        };


        try {
            const result = await this.dspClient._fetch({
                url: this.tokenEndpointUri,
                method: "POST",
                headers: {
                    "Content-Type": "application/x-www-form-urlencoded;charset=utf-8"
                },
                data: requestParam,
            });
            const token: IToken = JSON.parse(result.data);
            this._fireAuthorizedEvent();
            return token;
        }
        catch (error) {
            if (orgIsAuthorized)
                this._fireAuthorizedEvent();
            throw error;
        }
    }

    //signOut but keep current username
    public signOut(clearUser: boolean = true, redirect: boolean = false): void {

        // clear access_tokens
        this.setTokens(null);

        // clear all user info
        if (clearUser) this._resetUser();

        //redirect to signout page
        if (redirect && this.redirectUri) {
            let params = {
                client_id: this.clientId,
                redirect_uri: this.redirectUri,
                scope: this.scope,
                response_type: this.type,
                state: this.dspClient.sessionState
            };

            this.isAutoSignIn = false; //let leave the page
            if (this.logoutEndpointUri) {
                const uri: string = this.logoutEndpointUri + "?" + Convert.toQueryString(params);
                this.dspClient.control.navigate(uri);
            }
        }

        // always fire AuthorizedEvent
        this._fireAuthorizedEvent();
    };

    public grantAuthorization(password: string): void {
        if (!this.authorizeEndpointUri)
            throw new DirectSpError("authorizeEndpointUr has not been set!");

        let requestParam: any = this.authRequest;
        requestParam.SpApp_Authorization = this.authorizationHeader;
        requestParam.permission = "grant";
        if (password != null) requestParam.password = password;

        //submit
        let url: string = this.authorizeEndpointUri;
        if (this.dspClient.originalUri)
            url = url + this.dspClient.originalUri.search;
        Html.submit(url, requestParam);
    };

    public denyAuthorization(): void {
        if (!this.authorizeEndpointUri)
            throw new DirectSpError("authorizeEndpointUr has not been set!");

        let requestParam: any = this.authRequest;
        requestParam.SpApp_Authorization = this.authorizationHeader;
        requestParam.permission = "deny";


        //submit
        let url: string = this.authorizeEndpointUri;
        if (this.dspClient.originalUri)
            url = url + this.dspClient.originalUri.search;
        Html.submit(url, requestParam);
    };

    //navigate to directSp auth server
    public signIn(): void {
        //save current location
        if (this.dspClient.control.location)
            this.dspClient.dspSessionStorage.setItem(this.storageNamePrefix + "lastPageUri", this.dspClient.control.location.href);

        //redirect to sign in
        let params = {
            client_id: this.clientId,
            redirect_uri: this.redirectUri,
            scope: this.scope,
            response_type: this.type,
            state: this.dspClient.sessionState
        };
        const url: string = this.authorizeEndpointUri + "?" + Convert.toQueryString(params);
        this.dspClient.control.navigate(url);
    };
}
