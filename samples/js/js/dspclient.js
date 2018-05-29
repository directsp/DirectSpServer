/*
 * Website: https://github.com/directsp
 */

/*
 * Events:
 *  onAuthorized    * check the authorization state
 *  onError         * handling herror
 */
"use strict";

//namespace
if (!directSp) var directSp = {};

directSp.DirectSpError = function (error) {

    let message = '';
    if (error.errorName) message += error.errorName + "; "
    if (error.errorMessage) message += error.errorMessage + "; ";
    if (error.errorDescription) message += error.errorDescription + "; ";
    message = message.replace(/; $/, ''); //remove last simicolon

    let err = new Error(message);
    Object.setPrototypeOf(err, directSp.DirectSpError.prototype);

    err.errorType = directSp.Utility.checkUndefined(error.errorType);
    err.errorName = directSp.Utility.checkUndefined(error.errorName);
    err.errorNumber = directSp.Utility.checkUndefined(error.errorNumber);
    err.errorMessage = directSp.Utility.checkUndefined(error.errorMessage);
    err.errorDescription = directSp.Utility.checkUndefined(error.errorDescription);
    err.errorProcName = directSp.Utility.checkUndefined(error.errorProcName);
    err.errorData = directSp.Utility.checkUndefined(error.errorData);
    err.status = directSp.Utility.checkUndefined(error.status);
    err.statusText = directSp.Utility.checkUndefined(error.statusText);
    err.innerError = directSp.Utility.checkUndefined(error.innerError);

    return err;
}

directSp.DirectSpError.prototype = Object.create(Error.prototype, { name: { value: 'DirectSpError', enumerable: false } });

//DirectSpClient
directSp.DirectSpClient = function () {

    this._settings = {
        clientId: "",
        homePageUri: window.location.origin,
        authRedirectUri: window.location.origin + "/oauth2/callback",
        authEndpointUri: null,
        tokenEndpointUri: null,
        userinfoEndpointUri: null,
        logoutEndpointUri: null,
        authScope: "offline_access", //openid offline_access profile phone email address
        authType: "token", //token, code
        resourceApiUri: null,
        isPersistentSignIn: true,
        isAutoSignIn: false,
        isLogEnabled: true,
        isUseAppErrorHandler: false,
        refreshClockSkew: 60
    };

    this._authRequest = {
        client_id: directSp.Uri.getParameterByName('client_id'),
        redirect_uri: directSp.Uri.getParameterByName('redirect_uri'),
        scope: directSp.Uri.getParameterByName('scope'),
        response_type: directSp.Uri.getParameterByName('response_type'),
        state: directSp.Uri.getParameterByName('state')
    };

    this._userInfoLast = {
        username: null,
        name: null
    };

    this._storageNamePrefix = "DirectSp:";
    this._tokens = null; //{ access_token: "", expires_in: 0, refresh_token: "", token_type: "" },
    this._sessionState = Math.floor(Math.random() * 10000000000000);
    this._resourceAppVersion = localStorage.getItem(this._storageNamePrefix + "resouceAppVersion");
    this._lastPageUri = null;
    this._invokeHook = function (hookParams) { return null; }
    this._authError = null;
    this._userInfo = null;
    this._userInfoLast = null;
    this._accessTokenInfo = null;
    this._onAuthorized = null;
    this._onError = null;
    this._originalUri = location.href;
    this._originalQueryString = location.search;
    this._systemApi = null;
    this._load();

};

directSp.DirectSpClient.prototype = {
    get userInfo() {
        return this._userInfo;
    },

    get accessTokenInfo() {
        return this._accessTokenInfo;
    },

    get userId() {
        return this.accessTokenInfo ? this.accessTokenInfo.sub : null;
    },

    get username() {
        let uInfo = this._userInfo ? this._userInfo : this._userInfoLast;
        return uInfo ? uInfo.username : null;
    },

    get userDisplayName() {
        let uInfo = this._userInfo ? this._userInfo : this._userInfoLast;
        return uInfo ? uInfo.name : uInfo.username;
    },

    get clientId() {
        return this._settings.clientId;
    },
    set clientId(value) {
        this._settings.clientId = value;
    },

    get authError() {
        return this._authError;
    },

    get homePageUri() {
        return this._settings.homePageUri;
    },
    set homePageUri(value) {
        this._settings.homePageUri = value;
    },

    //check is client token exists and user has already signed it. The token may not valid
    get isAutoSignIn() {
        return this._settings.isAutoSignIn;
    },
    set isAutoSignIn(value) {
        this._settings.isAutoSignIn = value;
    },
    get isUseAppErrorHandler() {
        return this._settings.isUseAppErrorHandler;
    },
    set isUseAppErrorHandler(value) {
        this._settings.isUseAppErrorHandler = value;
    },
    get isAuthorized() {
        return this._tokens != null;
    },
    get isPersistentSignIn() {
        return this._settings.isPersistentSignIn;
    },
    set isPersistentSignIn(value) {
        this._settings.isPersistentSignIn = value;
    },
    get resourceApiUri() {
        return this._settings.resourceApiUri;
    },
    set resourceApiUri(value) {
        this._settings.resourceApiUri = value;
    },
    get authBaseUri() {
        return this._settings.authBaseUri;
    },
    set authBaseUri(value) {
        this._settings.authBaseUri = value;
        this._settings.authEndpointUri = directSp.Uri.combine(value, "/connect/authorize");
        this._settings.tokenEndpointUri = directSp.Uri.combine(value, "/connect/token");
        this._settings.userinfoEndpointUri = directSp.Uri.combine(value, "/connect/userinfo");
        this._settings.logoutEndpointUri = directSp.Uri.combine(value, "/connect/logout");
    },
    get authEndpointUri() {
        return this._settings.authEndpointUri;
    },
    set authEndpointUri(value) {
        this._settings.authEndpointUri = value;
    },
    get tokenEndpointUri() {
        return this._settings.tokenEndpointUri;
    },
    set tokenEndpointUri(value) {
        this._settings.authEndpointUri = value;
    },
    get userinfoEndpointUri() {
        return this._settings.userinfoEndpointUri;
    },
    set userinfoEndpointUri(value) {
        this._settings.userinfoEndpointUri = value;
    },
    get logoutEndpointUri() {
        return this._settings.logoutEndpointUri;
    },
    set logoutEndpointUri(value) {
        this._settings.logoutEndpointUri = value;
    },
    get authRedirectUri() {
        return this._settings.authRedirectUri;
    },
    set authRedirectUri(value) {
        this._settings.authRedirectUri = value;
    },
    get authScope() {
        return this._settings.authScope;
    },
    set authScope(value) {
        this._settings.authScope = value;
    },
    get authType() {
        return this._settings.authType;
    },
    set authType(value) {
        this._settings.authType = value;
    },
    get tokens() {
        return this._tokens;
    },
    get onAuthorized() {
        return this._onAuthorized;
    },
    set onAuthorized(value) {
        this._onAuthorized = value;
    },
    get onError() {
        return this._onError;
    },
    set onError(value) {
        this._onError = value;
        this.isUseAppErrorHandler = true;
    },
    get authHeader() {
        return this.tokens != null ? this.tokens.token_type + ' ' + this.tokens.access_token : null;
    },
    get authRequest() {
        return this._authRequest;
    },
    get authRequestUri() {
        return window.location.origin + "?" + directSp.Convert.toQueryString(this._authRequest);
    },
    get isAuthRequest() {
        return this.authRequest.client_id != null && this.authRequest.state != null;
    },
    get isLogEnabled() {
        return this._settings.isLogEnabled;
    },
    set isLogEnabled(value) {
        this._settings.isLogEnabled = value;
    },
    get isAuthCallback() {
        let callbackPattern = this.authRedirectUri;
        return callbackPattern != null && location.href.indexOf(callbackPattern) != -1;
    },
    get invokeHook() {
        return this._invokeHook;
    },
    set invokeHook(value) {
        this._invokeHook = value;
    },
    get originalQueryString() {
        return this._originalQueryString;
    },
    get originalUri() {
        return this._originalUri;
    }
};

//navigate to directSp auth server
directSp.DirectSpClient.prototype.init = function () {
    console.log("DirectSp: initializing ...");

    //process error
    if (directSp.Uri.getParameterByName("error") != null) {
        this._authError = this._convertToError({
            error: directSp.Uri.getParameterByName("error"),
            error_description: directSp.Uri.getParameterByName("error_description")
        });

        console.error("DirectSp: Auth Error!", this._authError);
        if (this.isAutoSignIn) {
            this.isAutoSignIn = false;
            console.warn("DirectSp: isAutoSignIn is set to false due Auth Error", this.authError);
        }
    }

    return this._processAuthCallback()
        .then(result => {
            if (!this.isAuthorized && this._tokensLast)
                return this.setTokens(this._tokensLast);
            return result;
        })
        .then(result => {
            this._fireAuthorizedEvent();
            return result;
        })
        .catch(error => {
            this._fireAuthorizedEvent();
            throw error;
        });
};

directSp.DirectSpClient.prototype._getInvokeOptionsFromAjaxOptions = function (ajaxOptions) {
    return ajaxOptions && ajaxOptions.data && ajaxOptions.data.invokeOptions ? ajaxOptions.data.invokeOptions : null;
};

directSp.DirectSpClient.prototype.setTokens = function (value, refreshUserInfo) {
    // defaults
    refreshUserInfo = directSp.Convert.toBoolean(refreshUserInfo, true);

    // check is token changed
    if (value == this._tokens)
        return Promise.resolve(value); //no change

    //set token
    this._tokens = value;

    //update auth data
    if (value != null) {
        //clear token if it was invalid
        try {
            this._accessTokenInfo = directSp.Utility.parseJwt(this.tokens.access_token);
        } catch (error) {
            return this.setTokens(null);
        }

        //refreshing UserInfo
        if (refreshUserInfo) {
            return this._refreshUserInfo()
                .then(result => {
                    this._save();
                    return value;
                });
        }
    }
    else {
        this._userInfo = null;
    }

    this._save();
    return Promise.resolve(value);
};


directSp.DirectSpClient.prototype.createError = function (error) {
    return this._convertToError(error); //fix error
};

directSp.DirectSpClient.prototype.throwAppError = function (error) {
    if (this._onError)
        this._onError(this._convertToError(error)); //fix error
};


directSp.DirectSpClient.prototype._fireAuthorizedEvent = function () {

    setTimeout(() => {
        //fire the event
        if (this.isAuthorized)
            console.log("DirectSp: User has been authorized", this.userInfo);
        else
            console.log("DirectSp: User has not been authorized!");

        if (!this.isAuthorized && this.isAutoSignIn) {
            this.signIn();
            return;
        }

        //trigger the event
        if (this.onAuthorized) {
            let data = { lastPageUri: this._lastPageUri };
            this.onAuthorized(data);
        }

        this._lastPageUri = null; //reset last page notifying user
        sessionStorage.removeItem(this._storageNamePrefix + "lastPageUri");
    }, 0);
};

directSp.DirectSpClient.prototype._resetUser = function () {
    this._tokens = null;
    this._userInfo = null;
    this._userInfoLast = null;
    this._save();
};

directSp.DirectSpClient.prototype._isTokenExpiredError = function (error) {
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

directSp.DirectSpClient.prototype._refreshUserInfo = function () {

    return this.getUserInfo()
        .then(result => {
            this._userInfo = result;
            this._userInfoLast = {
                username: result.username,
                userDisplayName: result.name
            };
            return true;
        })
        .catch(error => {
            return false;
        });
};

directSp.DirectSpClient.prototype.getUserInfo = function () {
    if (!this.isAuthorized) {
        let data = { errorName: "unauthorized", errorMessage: "Can not refresh token for unauthorized users" };
        return Promise.reject(this._convertToError(data));
    }

    let ajaxOptions = {
        url: this.userinfoEndpointUri,
        method: "GET",
        headers: { "authorization": this.authHeader },
    };

    return this._ajax(ajaxOptions)
        .then(result => {
            result = JSON.parse(result);
            if (this.isLogEnabled) console.log("DirectSp: userInfo", result);
            return result;
        });
};

directSp.DirectSpClient.prototype._refreshToken = function () {

    //check expiration time
    if (this.accessTokenInfo && this.accessTokenInfo["exp"]) {
        let dateNow = new Date();
        if (parseInt(this.accessTokenInfo["exp"]) - this._settings.refreshClockSkew > dateNow.getTime() / 1000)
            return Promise.resolve(false); //token is not refreshed
    }

    //return false if token not exists
    if (!this.tokens || !this.tokens.refresh_token || !this.accessTokenInfo) {
        return Promise.resolve(false); //current token is not valid and can not be refreshed
    }

    //Refreshing token
    console.log("DirectSp: Refreshing current token ...");

    //create request param
    let requestParam = {
        grant_type: "refresh_token",
        refresh_token: this.tokens.refresh_token,
        client_id: this.clientId
    };

    //call
    let request = {
        url: this.tokenEndpointUri,
        method: "POST",
        headers: {
            "Content-Type": "application/x-www-form-urlencoded;charset=UTF-8",
            "authorization": this.authHeader
        },
        data: requestParam,
    };

    return this._ajaxProvider(request)
        .then(result => {
            result = JSON.parse(result);
            return this.setTokens(result, false);
        })
        .catch(error => {
            // do not clear token and let caller knows that session is authorized but token is expired
            // this.setTokens(null); do not unmark
            throw error;
        });
};

directSp.DirectSpClient.prototype._load = function () {
    try {
        //restore tokens
        let tokensString = sessionStorage.getItem(this._storageNamePrefix + "auth.tokens");
        if (!tokensString)
            tokensString = localStorage.getItem(this._storageNamePrefix + "auth.tokens");
        if (tokensString)
            this._tokensLast = JSON.parse(tokensString);

        //save lastUser
        let userStringInfo = localStorage.getItem(this._storageNamePrefix + "userInfoLast");
        if (userStringInfo)
            this._userInfoLast = JSON.parse(userStringInfo);

        //restore isPersistentSignIn if it is not set by caller
        let isPersistentSignIn = localStorage.getItem(this._storageNamePrefix + "isPersistentSignIn");
        if (isPersistentSignIn != null) this._settings.isPersistentSignIn = directSp.Convert.toBoolean(isPersistentSignIn);

        //load sessionState; use current session if there is not session
        let sessionState = sessionStorage.getItem(this._storageNamePrefix + "sessionState");
        if (sessionState != null)
            this._sessionState = sessionState;
        else
            sessionStorage.setItem(this._storageNamePrefix + "sessionState", this._sessionState);

    } catch (err) {
    }
};

directSp.DirectSpClient.prototype._save = function () {

    //save tokens
    let tokenString = JSON.stringify(this._tokens);
    sessionStorage.setItem(this._storageNamePrefix + "auth.tokens", tokenString);
    if (this.isPersistentSignIn)
        localStorage.setItem(this._storageNamePrefix + "auth.tokens", tokenString);
    else
        localStorage.removeItem(this._storageNamePrefix + "auth.tokens");

    //save lastUser login
    let userInfoLastString = JSON.stringify(this._userInfoLast);
    if (this._lastUser)
        localStorage.setItem(this._storageNamePrefix + "userInfoLast", userInfoLastString);
    else
        localStorage.removeItem(this._storageNamePrefix + "userInfoLast");


    //save isPersistentSignIn
    localStorage.setItem(this._storageNamePrefix + "isPersistentSignIn", this._settings.isPersistentSignIn);

    //save sessionState
    sessionStorage.setItem(this._storageNamePrefix + "sessionState", this._sessionState);
};

directSp.DirectSpClient.prototype.signInByPasswordGrant = function (username, password) {
    let orgIsAuthorized = this.isAuthorized;

    //clear user info and tokens
    if (this.username != username) {
        this._resetUser();
    }

    //save as last user
    this._userInfoLast = {
        username: username
    };

    //create request param
    let requestParam = {
        username: username,
        password: password,
        grant_type: "password",
        scope: this.authScope,
        client_id: this.clientId
    };

    return this._ajax(
        {
            url: this.tokenEndpointUri,
            headers: { "Content-Type": "application/x-www-form-urlencoded;charset=UTF-8" },
            data: requestParam,
            method: "POST"
        })
        .then(result => {
            result = JSON.parse(result);
            return this.setTokens(result);
        })
        .then(result => {
            this._fireAuthorizedEvent();
            return result;
        })
        .catch(result => {
            if (orgIsAuthorized)
                this._fireAuthorizedEvent();
            throw result;
        });
};

//signOut but keep current username
directSp.DirectSpClient.prototype.signOut = function (clearUser, redirect) {
    // set default
    redirect = directSp.Convert.toBoolean(redirect, true);
    clearUser = directSp.Convert.toBoolean(clearUser, false);

    // clear access_tokens
    this.setTokens(null);

    // clear all user info
    if (clearUser)
        this._resetUser();

    //redirect to signout page
    if (redirect && this.authRedirectUri) {
        let params = {
            client_id: this.clientId,
            redirect_uri: this.authRedirectUri,
            scope: this.authScope,
            response_type: this.authType,
            state: this._sessionState
        };

        this.isAutoSignIn = false; //let leave the page
        window.location.href = this.logoutEndpointUri + "?" + directSp.Convert.toQueryString(params);
    }

    // always fire AuthorizedEvent
    this._fireAuthorizedEvent();
    return Promise.resolve();
};

directSp.DirectSpClient.prototype.grantAuthorization = function (password) {
    let requestParam = this.authRequest;
    requestParam.SpApp_Authorization = this.authHeader;
    requestParam.permission = "grant";
    if (password != null) requestParam.password = password;
    directSp.Html.submit(this.authEndpointUri + this._originalQueryString, requestParam);
};

directSp.DirectSpClient.prototype.denyAuthorization = function () {
    let requestParam = this.authRequest;
    requestParam.SpApp_Authorization = this.authHeader;
    requestParam.permission = "deny";
    directSp.Html.submit(this.authEndpointUri + this._originalQueryString, requestParam);
};

//navigate to directSp auth server
directSp.DirectSpClient.prototype.signIn = function () {

    //save current location
    sessionStorage.setItem(this._storageNamePrefix + "lastPageUri", window.location.href);

    //redirect to sign in
    let params = {
        client_id: this.clientId,
        redirect_uri: this.authRedirectUri,
        scope: this.authScope,
        response_type: this.authType,
        state: this._sessionState
    };
    window.location.href = this.authEndpointUri + "?" + directSp.Convert.toQueryString(params);
};

//navigate to directSp authorization server
// data will be true if 
directSp.DirectSpClient.prototype._processAuthCallback = function () {

    //check is oauth2callback
    if (!this.isAuthCallback)
        return Promise.resolve(false);

    // restore last pageUri
    this._lastPageUri = sessionStorage.getItem(this._storageNamePrefix + "lastPageUri");
    if (this._lastPageUri == null)
        this._lastPageUri = this.homePageUri;

    //check state and do nothing if it is not matched
    let state = directSp.Uri.getParameterByName("state");
    if (this._sessionState != state) {
        console.error("DirectSp: Invalid sessionState!");
        this.setTokens(null);
        return Promise.reject(this.createError("Invalid sessionState!"));
    }

    //process authorization_code flow
    let code = directSp.Uri.getParameterByName("code");
    if (code != null) {
        let requestParam = {
            client_id: this.clientId,
            redirect_uri: this.authRedirectUri,
            grant_type: "authorization_code",
            code: code
        };

        return this._ajax(
            {
                url: this.tokenEndpointUri,
                headers: { "Content-Type": "application/x-www-form-urlencoded;charset=UTF-8" },
                data: requestParam,
                method: "POST",
            })
            .then(result => {
                result = JSON.parse(result);
                return this.setTokens(result);
            })
            .catch(error => {
                this.setTokens(null);
                throw error;
            });
    }

    //process implicit flow
    let access_token = directSp.Uri.getParameterByName("access_token");
    if (access_token != null) {
        return this.setTokens({
            access_token: access_token,
            token_type: directSp.Uri.getParameterByName("token_type"),
            expires_in: directSp.Uri.getParameterByName("expires_in")
        });
    }

    //finish processAuthCallback without any result
    return Promise.resolve(false);
};

directSp.DirectSpClient.prototype._convertToError = function (data) {

    let error = {};

    //casting data
    if (data == null) {
        error.errorName = "unknown";
    }
    else if (typeof data == "number") {
        error.errorType = "number";
        error.errorNumber = data;
    }
    else if (typeof data == "string") {
        error.errorType = "string";
        error.errorName = data;
    }
    else if (typeof data != "object") {
        error.errorName = data;
    }
    else if (data.errorName != null || data.errorNumber != null || data.error != null || data.error_description != null) {
        error = data;
    }
    else {
        error.errorName = "unknown";
        error.errorDescription = data.toString();
        error.innerError = data;
    }

    //try to convert error
    if (error.error != null) {
        error.errorType = "OpenIdConnect";
        error.errorName = error.error;
        delete error["error"];
    }

    //try to convert error_description
    if (error.error_description != null) {
        error.errorDescription = error.error_description;
        delete error["error_description"];
    }

    //try to reconver actual data from error_description
    if (error.errorDescription != null) {
        try {
            let obj = JSON.parse(error.errorDescription);
            if (obj.errorName != null || obj.errorNumber != null)
                error = obj;
        } catch (e) {
        }
    }

    return new directSp.DirectSpError(error);
};

directSp.DirectSpClient.prototype.invokeBatch = function (spCalls, invokeOptions) {

    let invokeParamsBatch = {
        spCalls: spCalls,
        invokeOptions: invokeOptions
    };

    return this._invokeCore("invokeBatch", invokeParamsBatch);
};

//invokeOptions {pagination:"none|client|server", pageSize:10, pageIndex:0}
directSp.DirectSpClient.prototype.invoke = function (method, params, invokeOptions) {

    let spCall = {
        method: method,
        params: params
    };

    return this.invoke2(spCall, invokeOptions);
};

directSp.DirectSpClient.prototype.invoke2 = function (spCall, invokeOptions) {
    //validate
    if (spCall == null) throw "spCall is expected";
    if (spCall.method == null) throw "method is expected";
    if (spCall.params == null) spCall.params = {};
    if (this.resourceApiUri == null) throw "dspClient.resourceApiUri is not set";

    //set defaults
    if (invokeOptions == null) invokeOptions = {};
    if (invokeOptions.autoDownload == true) {
        invokeOptions.isWithRecodsetDownloadUri = true;
        if (!invokeOptions.recordCount) invokeOptions.recordCount = -2;
        if (!invokeOptions.recordsetFormat) invokeOptions.recordsetFormat = "tabSeparatedValues";
    }

    //use Paginator
    let res = this._processPagination(spCall, invokeOptions);
    if (res != null)
        return res;

    // create invokerParams
    let invokeParams = {
        spCall: spCall,
        invokeOptions: invokeOptions
    }

    //call api
    return this._invokeCore(spCall.method, invokeParams)
        .then(result => {
            if (invokeOptions.autoDownload)
                window.location = result.recordsetUri;
            return result;
        });
};

// AppErrorHandler
directSp.DirectSpClient.prototype._invokeCore = function (method, invokeParams) {

    //set defaults
    if (!invokeParams.invokeOptions) invokeParams.invokeOptions = {};
    invokeParams.invokeOptions.cache == directSp.Convert.toBoolean(invokeParams.invokeOptions.cache, true);
    invokeParams.invokeOptions.isUseAppErrorHandler = directSp.Convert.toBoolean(invokeParams.invokeOptions.isUseAppErrorHandler, this.isUseAppErrorHandler);

    //log request
    if (this.isLogEnabled)
        console.log("DirectSp: invoke (Request)", method, invokeParams);

    return this._invokeCore2(method, invokeParams)
        .then(result => {
            //log response
            if (this.isLogEnabled)
                console.log("DirectSp: invoke (Response)", method, invokeParams, result);

            return result;
        })
        .catch(error => {
            if (this.isLogEnabled)
                console.warn("DirectSp: invoke (Response)", method, invokeParams, error);
            throw error;
        });
}

//Handle Hook and delay
directSp.DirectSpClient.prototype._invokeCore2 = function (method, invokeParams) {

    // manage hook
    let hookParams = {
        method: method,
        invokeParams: invokeParams,
        delay: 0,
    };

    let promise = this._processInvokeHook(hookParams);
    if (promise == null)
        promise = this._invokeCore3(method, invokeParams);

    //return the promise if there is no api delay
    if (hookParams.delay == null || hookParams.delay <= 0)
        return promise;

    //proces delay
    return new Promise((resolve, reject) => {
        let interval = hookParams.delay;
        let delay = directSp.Utility.getRandomInt(interval / 2, interval + interval / 2);
        console.warn('DirectSp: Warning! ' + method + ' is delayed by ' + delay + ' milliseconds');
        setTimeout(() => resolve(), delay);
    }).then(result => {
        return promise;
    });
};

directSp.DirectSpClient.prototype._invokeCore3 = function (method, invokeParams) {

    return this._ajax(
        {
            url: directSp.Uri.combine(this.resourceApiUri, method),
            data: invokeParams,
            method: "POST",
            headers: {
                "authorization": this.authHeader,
                "Content-Type": "application/json;charset=UTF-8",
            },
            cache: invokeParams.invokeOptions.cache,
        }).then(result => {
            return JSON.parse(result);
        });
};

// Automatic Error Handling
directSp.DirectSpClient.prototype._ajax = function (ajaxOptions) {

    return this._ajax2(ajaxOptions)
        .catch(error => {
            // create error controller
            let errorControllerOptions = {
                error: error,
                dspClient: this,
                ajaxOptions: ajaxOptions
            }
            let errorController = new directSp.ErrorController(errorControllerOptions);
            let invokeOptions = this._getInvokeOptionsFromAjaxOptions(ajaxOptions);
            let isUseAppHandler = invokeOptions && invokeOptions.isUseAppErrorHandler;

            //we should call onError if the exception can be retried
            if (this.onError && (errorController.canRetry || isUseAppHandler)) {
                if (this.isLogEnabled) console.log("DirectSp: Calling onError ...");
                this.onError(errorController);
            }

            return errorController.promise;
        });
};

//manageToken
directSp.DirectSpClient.prototype._ajax2 = function (ajaxOptions) {

    if (!ajaxOptions.headers || !ajaxOptions.headers.authorization)
        return this._ajaxProvider(ajaxOptions); //there is no token

    return this._refreshToken()
        .then(result => {
            ajaxOptions.headers.authorization = this.authHeader;
            return this._ajaxProvider(ajaxOptions);
        })
        .catch(error => {
            // signout if token is expired
            if (this._isTokenExpiredError(error) && this.isAuthorized) {
                return this.signOut()
                    .then(result => {
                        throw error;
                    });
            }
            throw error;
        });
};

directSp.DirectSpClient.prototype._ajaxProvider = function (ajaxOptions) {

    return new Promise((resolve, reject) => {
        let req = new XMLHttpRequest();
        req.withCredentials = ajaxOptions.withCredentials;
        req.open(ajaxOptions.method, ajaxOptions.url);
        req.onload = () => {
            this._checkNewVersion(req.getResponseHeader("DSP-AppVersion"));

            if (req.status == 200) {
                resolve(req.responseText);
            }
            else {
                let error = null;
                try {
                    let obj = JSON.parse(req.responseText);
                    error = this._convertToError(obj);
                    error.innerError = obj;
                } catch (err) {
                    let text = req.responseText == "" ? req.statusText : req.responseText;
                    error = this._convertToError(text);
                }

                error.status = req.status;
                error.statusText = req.statusText;
                reject(error);
            }
        };
        req.onerror = () => {
            this._checkNewVersion(req.getResponseHeader("DSP-AppVersion"));
            reject(this.createError({ errorName: "Network Error", errorDescription: "Network error or server unreachable!", errorNumber: 503 }));
        };

        //headers
        if (ajaxOptions.headers) {
            for (let item in ajaxOptions.headers) {
                if (ajaxOptions.headers.hasOwnProperty(item))
                    req.setRequestHeader(item, ajaxOptions.headers[item]);
            }
        }

        //creating body
        let body = ajaxOptions.data;

        //finding Content-Type
        let contentType = ajaxOptions.headers ? ajaxOptions.headers["Content-Type"] : null;
        if (!contentType) contentType = 'application/x-www-form-urlencoded;charset=UTF-8' //default
        contentType = contentType.toLowerCase();

        //convert data based on contentType
        if (contentType.indexOf("application/json") != -1 && body)
            body = JSON.stringify(ajaxOptions.data);
        else if (contentType.indexOf("application/x-www-form-urlencoded") != -1)
            body = directSp.Convert.toQueryString(ajaxOptions.data);

        //send
        req.send(body);
    });
};

directSp.DirectSpClient.prototype._checkNewVersion = function (resourceAppVersion) {
    // app versin does not available if resourceAppVersion is null
    if (!resourceAppVersion || resourceAppVersion == this._resourceAppVersion)
        return;

    //detect new versio
    let isReloadNeeded = this._resourceAppVersion != null && this._resourceAppVersion != resourceAppVersion;

    // save new version
    this._resourceAppVersion = resourceAppVersion;
    localStorage.setItem(this._storageNamePrefix + "resouceAppVersion", resourceAppVersion);

    // reloading
    if (isReloadNeeded) {
        console.log("DirectSp: New version detected! Reloading ...");
        window.location.reload(true);
    }

    return isReloadNeeded;
}

directSp.DirectSpClient.prototype._processInvokeHook = function (hookParams) {

    //return quickly if there is no hook
    if (!this.invokeHook)
        return null;

    //run hook
    try {
        let promise = this.invokeHook(hookParams);
        if (promise == null)
            return null;

        //log hook
        if (this.isLogEnabled)
            console.warn("DirectSp: Hooking > ", hookParams.method, hookParams);

        return promise.then(result => {
            //clone data
            result = directSp.Utility.clone(result);

            //support paging
            let invokeParams = hookParams.invokeParams;
            let invokeOptions = invokeParams.invokeOptions;
            if (invokeOptions && invokeOptions.recordIndex != null && result.recordset) {
                if (invokeOptions.recordCount == null) invokeOptions.recordCount = 20;
                result.recordset = result.recordset.slice(invokeOptions.recordIndex, invokeOptions.recordIndex + invokeOptions.recordCount);
            }
            return result;
        });
    }
    catch (e) {
        return Promise.reject(this._convertToError(e)); //make sure hook error converted to Error
    }
}

directSp.DirectSpClient.prototype.help = function (criteria, reload) {
    reload = directSp.Utility.checkUndefined(reload, false);

    //Load Api info if it is not loaded
    if (!this._systemApi || reload) {
        this.invoke("System_Api")
            .then(result => {
                this._systemApi = result.api;
                if (result.api)
                    this._help(criteria);
                else
                    console.log("DirectSp: Could not retreive api information!");
            });
        return 'wait...';
    }


    return this._help(criteria);
}


directSp.DirectSpClient.prototype._help = function (criteria) {
    // show help
    if (criteria != null) criteria = criteria.trim().toLowerCase();
    if (criteria == "") criteria = null;

    //find all proc that match
    let foundProc = [];
    for (let i = 0; i < this._systemApi.length; i++) {
        if (criteria == null || this._systemApi[i].procedureName.toLowerCase().indexOf(criteria) != -1) {
            foundProc.push(this._systemApi[i]);
        }
    }

    //show procedure if there is only one procedure
    if (foundProc.length == 0) {
        console.log('DirectSp: Nothing found!');
    }
    else {
        for (let i = 0; i < foundProc.length; i++) {
            if (foundProc[i].procedureName.toLowerCase() == criteria || foundProc.length == 1)
                this._helpImpl(foundProc[i]);
            else
                console.log(foundProc[i].procedureName);
        }
    }

    return '---------------';
};

directSp.DirectSpClient.prototype._helpImpl = function (procedureMetadata) {
    // find max param length
    let maxParamNameLength = 0;
    let inputParams = [];
    for (let i = 0; i < procedureMetadata.params.length; i++) {
        let param = procedureMetadata.params[i];
        maxParamNameLength = Math.max(maxParamNameLength, param.paramName.length)
        if (!param.isOutput && param.paramName.toLowerCase() != '@context') {
            inputParams.push(this._formatHelpParamName(param.paramName));
        }
    }
    let s = {
        a: "",
        b: {}
    }
    //prepare input params
    let str = "";
    str += "\n" + '---------------';
    str += "\n" + "Method:";
    str += "\n\t" + procedureMetadata.procedureName + ' (' + inputParams.join(", ") + ')';
    str += "\n";
    str += "\n" + "Parameters:";
    for (let i = 0; i < procedureMetadata.params.length; i++) {
        let param = procedureMetadata.params[i];
        if (!param.isOutput && param.paramName.toLowerCase() != '@context') {
            str += "\n\t" + this._getHelpParam(procedureMetadata, param, maxParamNameLength);
        }
    }

    //prepare ouput params
    str += "\n";
    str += "\n" + "Returns:";
    for (let i = 0; i < procedureMetadata.params.length; i++) {
        let param = procedureMetadata.params[i];
        if (param.isOutput && param.paramName.toLowerCase() != '@context') {
            str += "\n\t" + this._getHelpParam(procedureMetadata, param, maxParamNameLength);
        }
    }
    str += "\n\t" + this._formatHelpParam("returnValue", "integer", maxParamNameLength);
    str += "\n\t" + this._formatHelpParam("recordset", "array", maxParamNameLength);

    //sample
    let sample = 'dspClient.invoke("$(procname)", { $(parameters) })';
    let sampleParam = [];
    for (let i = 0; i < inputParams.length; i++)
        sampleParam.push(inputParams[i] + ': ' + '$' + inputParams[i]);
    sample = sample.replace('$(procname)', procedureMetadata.procedureName);
    sample = sample.replace('$(parameters)', sampleParam.join(", "));
    str += "\n";
    str += "\n" + "Sample:";
    str += "\n\t" + sample;

    console.log(str)
}

directSp.DirectSpClient.prototype._getHelpParam = function (procedureMetadata, param, maxParamNameLength) {
    let paramType = this._getHelpParamType(procedureMetadata, param);
    return this._formatHelpParam(param.paramName, paramType, maxParamNameLength);
}

directSp.DirectSpClient.prototype._getHelpParamType = function (procedureMetadata, param) {

    //check userTypeName
    let userTypeName = param.userTypeName != null ? param.userTypeName.toLowerCase() : "";
    if (userTypeName.indexOf('json') != -1)
        return 'object';

    //check systemTypeName
    let paramType = param.systemTypeName.toLowerCase();
    if (paramType.indexOf("char") != -1) return "string";
    else if (paramType.indexOf("date") != -1) return "datetime";
    else if (paramType.indexOf("time") != -1) return "datetime";
    else if (paramType.indexOf("money") != -1) return "money";
    else if (paramType.indexOf("int") != -1) return "integer";
    else if (paramType.indexOf("float") != -1 || paramType.indexOf("decimal") != -1) return "float";
    else if (paramType.indexOf("bit") != -1 || paramType.indexOf("decimal") != -1) return "boolean";
    return "string"
};

directSp.DirectSpClient.prototype._formatHelpParamName = function (paramName) {
    //remove extra characters
    if (paramName.length > 0 && paramName[0] == '@')
        paramName = paramName.substr(1);
    return directSp.Utility.toCamelcase(paramName);
};

directSp.DirectSpClient.prototype._formatHelpParam = function (paramName, paramType, maxParamNameLength) {
    let str = this._formatHelpParamName(paramName);

    //add spaces
    for (let i = str.length; i < maxParamNameLength + 2; i++)
        str += ' ';

    return str + "(" + paramType + ")";
};

directSp.DirectSpClient.prototype._processPagination = function (spCall, invokeOptions) {

    //prevent recursive call
    if (invokeOptions.pagination == null && invokeOptions.pageSize != null) invokeOptions.pagination = "server";
    if (invokeOptions.pagination == null || invokeOptions.pagination == "" || invokeOptions.pagination == "none")
        return null;

    //create paginator
    let paginator = new directSp.DirectSpClient.Paginator(this, spCall, invokeOptions);
    return paginator;
};

// *********************
// **** Paginator
// *********************
directSp.DirectSpClient.Paginator = function (dspClient, spCall, invokeOptions) {

    this._dspClient = dspClient;
    this._apiCall = spCall;
    this._invokeOptions = invokeOptions;
    this._pagePromises = [];
    this._isInvoked = false;
    this._isCacheUsed = false;
    this.reset();
    this._isCacheInvalidated = false; //clear for first time after reset
    this._pageSize = invokeOptions.pageSize != null ? invokeOptions.pageSize : 20;
    this._pageCacheCount = (invokeOptions.pageCacheCount != null) ? invokeOptions.pageCacheCount : 1;

    if (this._pageSize < 1)
        this._dspClient.throwAppError("pageSize must be greater than 0");
};

directSp.DirectSpClient.Paginator.prototype = {
    get hasNextPage() {
        return (this.pageCountMax == null || (this.pageIndex + 1) < this.pageCountMax);
    },

    get hasPrevPage() {
        return this.pageIndex > 1;
    },

    get recordset() {
        let ret = this._pages[this.pageIndex];
        return ret != null ? ret : [];
    },

    get pageIndex() {
        return this._pageIndex;
    },

    get pageSize() {
        return this._pageSize;
    },

    set pageSize(value) {
        if (this._pageSize != value) {
            this._pageSize = value;
            this.reset();
        }
    },

    get pageCountMin() {
        return this.pageCount != null ? this.pageCount : this._pageCountMin;
    },

    get pageCountMax() {
        return this.pageCount != null ? this.pageCount : this._pageCountMax;
    },

    get pageCount() {
        if (this._recordCount !== null)
            return Math.max(Math.ceil(this._recordCount / this.pageSize), 1); //paginator always has a 1 page

        return this._pageCount;
    },

    get recordCount() {
        if (this._recordCount !== null)
            return this._recordCount;

        if (this.pageCount == null || this.pageCount == 0 || this._pages.length == 0 || !this._pages[this.pageCount - 1])
            return 0;

        return (this.pageCount - 1) * this.pageSize + this._pages[this.pageCount - 1].length;
    },

    get isCacheUsed() {
        return this._isCacheUsed;
    },

    get isCacheInvalidated() {
        return this._isCacheInvalidated;
    },

    get isInvoked() {
        return this._isInvoked;
    }
};

directSp.DirectSpClient.Paginator.prototype.downloadAsTsv = function () {
    let newInvokeOptions = {
        recordsetFileTitle: this._invokeOptions.recordsetFileTitle,
        autoDownload: true
    };

    let promise = this._dspClient.invoke2(this._apiCall, newInvokeOptions);
    return promise;
},

    directSp.DirectSpClient.Paginator.prototype.getApproxPageCount = function (maxPageCount) {
        let value = Math.max(maxPageCount, this.pageCountMin);
        if (this.pageCountMax != null)
            value = Math.min(value, this.pageCountMax);
        return value;
    },

    directSp.DirectSpClient.Paginator.prototype.goPrevPage = function () {
        return this.goPage(this.pageIndex + 1);
    };

directSp.DirectSpClient.Paginator.prototype.goNextPage = function () {
    return this.goPage(this.pageIndex - 1);
};

directSp.DirectSpClient.Paginator.prototype.refresh = function () {
    let curPageIndex = this.pageIndex;
    this.reset();
    return this.goPage(curPageIndex);
};

directSp.DirectSpClient.Paginator.prototype.reset = function () {
    this._pages = [];
    this._pageCount = null;
    this._pageCountMin = 1;
    this._pageCountMax = null;
    this._pageIndex = 0;
    this._isCacheInvalidated = true;
    this._isCacheUsed = false;
    this._recordCount = null;
};

directSp.DirectSpClient.Paginator.prototype._validatePageNo = function (pageNo) {
    if (!pageNo) pageNo = 0;
    if (pageNo == -1) pageNo = 0;
    if (this.pageCountMax != null) pageNo = Math.min(pageNo, this.pageCountMax - 1);
    return pageNo;
};


directSp.DirectSpClient.Paginator.prototype.goPage = function (pageNo) {
    //change current page after getting result
    pageNo = this._validatePageNo(pageNo);
    return this.getPage(pageNo)
        .then(result => {
            this._pageIndex = pageNo;
            return result;
        });
};

directSp.DirectSpClient.Paginator.prototype.getPage = function (pageNo) {
    this._isCacheInvalidated = false;
    this._isCacheUsed = true;
    this._isInvoked = false;
    pageNo = this._validatePageNo(pageNo);

    //find page range
    let pageStart = Math.max(0, pageNo - this._pageCacheCount);
    let pageEnd = pageNo + this._pageCacheCount;

    //validate page range
    if (this.pageCountMax != null) pageEnd = Math.min(pageEnd, this.pageCountMax - 1);
    pageStart = Math.min(pageStart, pageEnd); //page start can not be more than pageEnd

    //exclude cached pages from start
    for (; pageStart <= pageNo; pageStart++) {
        if (this._pages[pageStart] == null)
            break;
    }

    //exclude cached pages from end
    for (; pageEnd > pageNo; pageEnd--) {
        if (this._pages[pageEnd] == null)
            break;
    }
    let pageCount = pageEnd - pageStart + 1;

    if (pageCount > 0) {

        //calculate recourdIndex and recordCount
        let recordIndex = pageStart * this.pageSize;
        let recordCount = pageCount * this.pageSize + 1; //additional records to find last page
        if (pageStart > 0) {
            recordIndex--;
            recordCount++;
        }

        //remove pagination invokeOptions
        let invokeOptions = directSp.Utility.clone(this._invokeOptions);
        invokeOptions.pagination = "none";
        invokeOptions.recordIndex = recordIndex;
        invokeOptions.recordCount = recordCount;
        if (this._invokeOptions.pagination == "client") {
            invokeOptions.recordIndex = 0;
            invokeOptions.recordCount = -2;
        }

        //invoke
        this._isInvoked = true;
        let promise = this._dspClient.invoke2(this._apiCall, invokeOptions)
            .then(result => {

                let recordset = result.recordset != null ? result.recordset : [];

                //Detect record shift on left; Clear caches if the first record is not matched to last record of the previous page
                let prePageRecordset = pageStart > 0 ? this._pages[pageStart - 1] : null;
                if (prePageRecordset != null && (recordset.length == 0 || JSON.stringify(prePageRecordset[this.pageSize - 1]) != JSON.stringify(recordset[0])))
                    this.reset();

                //Detect record shift on right; Clear cache if the last record is not matched to first record of the previous page
                let nextPageRecordset = (pageEnd + 1 < this._pages.length) ? this._pages[pageEnd + 1] : null;
                if (nextPageRecordset != null && (recordset.length < recordCount || nextPageRecordset[0] != recordset[recordset.length - 1]))
                    this.reset();

                //estimate pageCount
                let lastRecordsetPageIndex = Math.floor((recordIndex + recordset.length) / this.pageSize);
                if ((recordIndex + recordset.length) % this.pageSize == 0) lastRecordsetPageIndex--;

                if (recordset.length == 0) {
                    if (pageStart <= this._pageCountMin) {
                        this.reset();
                    }

                    if (pageStart <= 1) {
                        this._recordCount = 0;
                        this._pageCount = 1;
                        this._pageCountMax = 1;
                    }

                    this._pageCountMax = Math.min(this._pageCountMax == null ? lastRecordsetPageIndex + 1 : this._pageCountMax, lastRecordsetPageIndex + 1);
                }
                else {
                    if (this._pageCountMax != null && pageStart >= this._pageCountMax) {
                        this.reset();
                    }

                    if (recordset.length != recordCount) {
                        this._pageCount = lastRecordsetPageIndex + 1;
                        this._pageCountMin = lastRecordsetPageIndex + 1;
                        this._pageCountMax = lastRecordsetPageIndex + 1;
                    }
                    else {
                        this._pageCountMin = Math.max(this._pageCountMin, lastRecordsetPageIndex + 1);
                    }
                }

                //assign pages
                let i = recordIndex + (recordIndex % this.pageSize) != 0 ? this.pageSize - (recordIndex % this.pageSize) : 0;
                for (; i < recordset.length; i = i + this.pageSize) {
                    let pageIndex = Math.floor((recordIndex + i) / this.pageSize);
                    let pageRecordset = recordset.slice(i, i + this.pageSize);

                    if (pageRecordset.length > 0 && (pageRecordset.length == this.pageSize || recordset.length < recordCount)) {
                        this._pages[pageIndex] = pageRecordset;
                        this._pagePromises[pageIndex] = promise;
                        if (pageNo == pageIndex) this._isCacheUsed = false;
                    }
                }

                // set pageCount if totalRecordCount exists
                if (result.totalRecordCount)
                    this._recordCount = result.totalRecordCount;

                return result;
            });

        //assign promises
        for (let i = pageStart; i <= pageEnd; i++)
            this._pagePromises[i] = promise;
    }

    //change current page after getting result
    return this._pagePromises[pageNo];
};

// *********************
// **** Error Controller
// *********************
directSp.ErrorController = function (data) {
    this._data = data;
    this._error = data.error;
    this._errorNumber = data.error ? data.error.errorNumber : null;

    //55022: InvalidCaptcha, 55027: Maintenance, 55028: MaintenanceReadOnly
    this._canRetry = this._errorNumber == 55022 || this._errorNumber == 55027 || this._errorNumber == 55028 || this._errorNumber == 503;

    if (this._errorNumber == 55022) {
        this.captchaImageUri = "data:image/png;base64," + data.error.errorData.captchaImage;
        this.captchaCode = null;
    };

    this.promise = new Promise((resolve, reject) => {
        this._resolve = resolve;
        this._reject = reject;
    });

    if (!this._canRetry)
        this._reject(data.error);
};

directSp.ErrorController.prototype = {
    get error() {
        return this._error;
    },

    get canRetry() {
        return this._canRetry;
    }
}

directSp.ErrorController.prototype.retry = function () {

    let ajaxOptions = this._data.ajaxOptions;
    let ajaxData = ajaxOptions.data;
    let invokeOptions = this._data.dspClient._getInvokeOptionsFromAjaxOptions(ajaxOptions);

    if (this._errorNumber == 55022) {

        //try update invokeParams for invoke
        if (invokeOptions) {
            invokeOptions.captchaId = this._data.captchaId;
            invokeOptions.captchaCode = this.captchaCode;
        }

        //try update param for authentication grantby password
        if (ajaxData.grant_type == "password") {
            ajaxData.captchaId = this.error.errorData.captchaId;
            ajaxData.captchaCode = this.captchaCode;
        }
    }

    // retry original ajax
    this._data.dspClient._ajax(ajaxOptions)
        .then(resolve => {
            return this._resolve(resolve);
        })
        .catch(error => {
            return this._reject(error);
        });
}

directSp.ErrorController.prototype.release = function () {
    let error = this._data.dspClient._convertToError("Operation has been canceled by the user!");
    this._reject(error);
}

//Utilities
directSp.Utility = {};

directSp.Utility.isDefined = function (obj) {
    return typeof obj != "undefined";
};
directSp.Utility.isUndefined = function (obj) {
    return typeof obj == "undefined";
};
directSp.Utility.checkUndefined = function (value, defValue) {
    if (directSp.Utility.isUndefined(defValue)) defValue = null;
    return typeof value != "undefined" ? value : defValue;
};

directSp.Utility.parseJwt = function (token) {
    let base64Url = token.split('.')[1];
    let base64 = base64Url.replace('-', '+').replace('_', '/');
    return JSON.parse(window.atob(base64));
};

directSp.Utility.clone = function (obj) {
    if (obj == null)
        return obj;
    return JSON.parse(JSON.stringify(obj));
};

directSp.Utility.shallowCopy = function (src, des) {
    for (let item in src) {
        if (src.hasOwnProperty(item))
            des[item] = src[item];
    }
};

directSp.Utility.deepCopy = function (src, des) {
    for (let item in src) {
        if (src.hasOwnProperty(item))
            des[item] = directSp.Utility.clone(src[item]);
    }
};

directSp.Utility.getRandomInt = function (min, max) {
    min = Math.ceil(min);
    max = Math.floor(max);
    return Math.floor(Math.random() * (max - min)) + min;
};

directSp.Utility.toCamelcase = function (str) {
    if (str == null || str == "")
        return str;

    return str.substr(0, 1).toLowerCase() + str.substr(1);
};

//return null if error occur
directSp.Utility.tryParseJason = function (json) {
    try {
        return JSON.parse(json);
    }
    catch (error) {
        return null;
    }
};


//Convert class
directSp.Convert = {};

directSp.Convert.toBoolean = function (value, defaultValue) {
    defaultValue = directSp.Utility.checkUndefined(defaultValue, false);
    if (directSp.Utility.isUndefined(value))
        return defaultValue;

    // check is value boolean
    if (typeof value == "boolean")
        return value;

    try {
        let parsed = parseInt(value);
        if (!isNaN(parsed) && parsed != 0)
            return true;
        switch (value.toLowerCase()) {
            case "true":
            case "yes":
            case "1":
                return true;
            case "false":
            case "no":
            case "0":
                return false;
        }
    }
    catch (err) {
    }
    return defaultValue;
};

directSp.Convert.toInteger = function (value, defaultValue) {
    defaultValue = directSp.Utility.checkUndefined(defaultValue, 0);
    try {
        let ret = parseInt(value);
        if (!isNaN(ret))
            return ret;
    }
    catch (err) {
    }
    return defaultValue;
};

directSp.Convert.toQueryString = function (obj) {
    let parts = [];
    for (let i in obj) {
        if (obj.hasOwnProperty(i)) {
            parts.push(encodeURIComponent(i) + "=" + encodeURIComponent(obj[i]));
        }
    }
    return parts.join("&");
};

//Uri Class
directSp.Uri = {};

directSp.Uri.getParameterByName = function (name, url) {
    if (!url) url = window.location.href;

    name = name.replace(/[\[\]]/g, "\\$&");
    let regex = new RegExp("[#?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
};

directSp.Uri.combine = function (uriBase, uriRelative) {

    let parts = [uriBase, uriRelative];
    return parts.map(function (path) {
        if (path[0] == "/") {
            path = path.slice(1);
        }
        if (path[path.length - 1] == "/") {
            path = path.slice(0, path.length - 1);
        }
        return path;
    }).join("/");
};

directSp.Uri.getParent = function (uri) {
    let endIndex = uri.indexOf("?");
    if (endIndex == -1) endIndex = uri.length;
    let lastSlash = uri.lastIndexOf("/", endIndex);
    return uri.substr(0, lastSlash);
};

directSp.Uri.getUrlWithoutQueryString = function (uri) {
    return uri.split("?").shift().split("#").shift();
};

directSp.Uri.getFileName = function (uri) {
    uri = directSp.Uri.getUrlWithoutQueryString(uri);
    return uri.split("/").pop();
};

// html class
directSp.Html = {};

directSp.Html.submit = function (url, params) {
    let method = "post";

    let form = document.createElement("form");
    form.setAttribute("method", method);
    form.setAttribute("action", url);

    for (let key in params) {
        if (params.hasOwnProperty(key)) {
            let hiddenField = document.createElement("input");
            hiddenField.setAttribute("type", "hidden");
            hiddenField.setAttribute("name", key);
            hiddenField.setAttribute("value", params[key]);

            form.appendChild(hiddenField);
        }
    }

    document.body.appendChild(form);
    form.submit();
};

//Create object
let dspClient = new directSp.DirectSpClient();
