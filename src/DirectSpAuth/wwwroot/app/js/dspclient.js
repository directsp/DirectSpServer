/*
 * Website: https://github.com/directsp
 */

/*
 * Events:
 *  onAuthorized    * check the authorization state
 *  onCaptcha       * request captcha
 */
"use strict";

//namespace
if (typeof directSp == "undefined") var directSp = {};

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
        sessionState: Math.floor(Math.random() * 10000000000000),
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

    this._tokens = null; //{ access_token: "", expires_in: 0, refresh_token: "", token_type: "" },
    this._lastPageUri = null;
    this._apiHook = function (method, params) { return null; }
    this._authError = null;
    this._userInfo = null;
    this._userInfoLast = null;
    this._accessTokenInfo = null;
    this._onAuthorized = null;
    this._onCaptcha = null;
    this._onError = null;
    this._storageNamePrefix = "DirectSp:";
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
        return accessTokenInfo ? accessTokenInfo.sub : null;
    },

    get username() {
        var uInfo = this._userInfo ? this._userInfo : this._userInfoLast;
        return uInfo ? uInfo.username : null;
    },

    get userDisplayName() {
        var uInfo = this._userInfo ? this._userInfo : this._userInfoLast;
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
    get onCaptcha() {
        return this._onCaptcha;
    },
    set onCaptcha(value) {
        this._onCaptcha = value;
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
        var callbackPattern = this.authRedirectUri;
        return callbackPattern != null && location.href.indexOf(callbackPattern) != -1;
    },
    get apiHook() {
        return this._apiHook;
    },
    set apiHook(value) {
        this._apiHook = value;
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
    var _this = this;

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
            if (!_this.isAuthorized && _this._tokensLast)
                return _this.setTokens(_this._tokensLast);
            return result;
        })
        .then(result => {
            _this._fireAuthorizedEvent();
            return result;
        })
        .catch(error => {
            _this._fireAuthorizedEvent();
            throw error;
        });
};

directSp.DirectSpClient.prototype.setTokens = function (value) {
    if (value == this._tokens)
        return; //no change

    //set token
    this._tokens = value;

    //update auth data
    if (value != null) {
        this._accessTokenInfo = directSp.Utility.parseJwt(this.tokens.access_token);
        var _this = this;
        return this._updateUserInfo()
            .then(result => {
                _this._save();
                return result;
            });
    }
    else {
        this._userInfo = null;
    }

    this._save();
    return Promise.resolve(true);
};


directSp.DirectSpClient.prototype.createError = function (error) {
    return this._convertToError(error); //fix error
};

directSp.DirectSpClient.prototype.throwAppError = function (error) {
    if (this._onError)
        this._onError(this._convertToError(error)); //fix error
};


directSp.DirectSpClient.prototype._fireAuthorizedEvent = function () {

    var _this = this;
    setTimeout(function () {
        //fire the event
        if (_this.isAuthorized)
            console.log("DirectSp: User has been authorized", _this.userInfo);
        else
            console.log("DirectSp: User has not been authorized!");

        if (!_this.isAuthorized && _this.isAutoSignIn) {
            _this.signIn();
            return;
        }

        //trigger the event
        if (_this.onAuthorized) {
            var data = { lastPageUri: _this._lastPageUri };
            _this.onAuthorized(data);
        }

        _this._lastPageUri = null; //reset last page notifying user
        sessionStorage.removeItem(_this._storageNamePrefix + "lastPageUri");
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

directSp.DirectSpClient.prototype._updateUserInfo = function () {

    var _this = this;
    return this.getUserInfo()
        .then(result => {
            _this._userInfo = result;
            _this._userInfoLast = {
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
        var data = { errorName: "unauthorized", errorMessage: "Can not refresh token for unauthorized users" };
        return Promise.reject(this._convertToError(data));
    }

    var _this = this;
    return this._ajax(
        {
            url: this.userinfoEndpointUri,
            data: null,
            type: "GET",
            headers: { 'authorization': this.authHeader },
            cache: false
        })
        .then(data => {
            if (_this.isLogEnabled) console.log("DirectSp: userInfo", data);
            return data;
        });
};

directSp.DirectSpClient.prototype._refreshToken = function () {

    //check expiration time
    if (this.accessTokenInfo && this.accessTokenInfo["exp"]) {
        var dateNow = new Date();
        if (int.parse(this.accessTokenInfo["exp"]) - this.refreshClockSkew > dateNow.getTime())
            return Promise.resolve(true); //current token is valid
    }

    //return false if token not exists
    if (!this.tokens || !this.tokens.refresh_token || !this.accessTokenInfo) {
        return Promise.resolve(false); //current token is not valid and can not be refreshed
    }


    //Refreshing token
    console.log("DirectSp: Refreshing current token ...");

    //create request param
    var requestParam = {
        grant_type: "refresh_token",
        refresh_token: this.tokens.refresh_token,
        client_id: this.clientId
    };

    //call 
    var _this = this;
    return this._ajax(
        {
            url: _this.tokenEndpointUri,
            data: requestParam,
            type: "POST",
            cache: false
        })
        .then(data => {
            return _this.setTokens(data);
        })
        .catch(error => {
            // do not clear token and let caller knows that session is authorized but token is expired
            // _this.setTokens(null); do not unmark
            throw error;
        });
};

directSp.DirectSpClient.prototype._load = function () {
    try {
        //restore tokens
        var tokensString = sessionStorage.getItem(this._storageNamePrefix + "auth.tokens");
        if (!tokensString)
            tokensString = localStorage.getItem(this._storageNamePrefix + "auth.tokens");
        if (tokensString)
            this._tokensLast = JSON.parse(tokensString);

        //save lastUser
        var userStringInfo = localStorage.getItem(this._storageNamePrefix + "userInfoLast");
        if (userStringInfo)
            this._userInfoLast = JSON.parse(userStringInfo);

        //restore isPersistentSignIn if it is not set by caller
        var isPersistentSignIn = localStorage.getItem(this._storageNamePrefix + "isPersistentSignIn");
        if (isPersistentSignIn != null) this._settings.isPersistentSignIn = directSp.Convert.toBoolean(isPersistentSignIn);

        //load sessionState; use current session if there is not session
        var sessionState = sessionStorage.getItem(this._storageNamePrefix + "sessionState");
        if (sessionState != null)
            this._settings.sessionState = sessionState;
        else
            sessionStorage.setItem(this._storageNamePrefix + "sessionState", this._settings.sessionState);

    } catch (err) {
    }
};

directSp.DirectSpClient.prototype._save = function () {

    //save tokens
    var tokenString = JSON.stringify(this._tokens);
    sessionStorage.setItem(this._storageNamePrefix + "auth.tokens", tokenString);
    if (this.isPersistentSignIn)
        localStorage.setItem(this._storageNamePrefix + "auth.tokens", tokenString);
    else
        localStorage.removeItem(this._storageNamePrefix + "auth.tokens");

    //save lastUser login
    var userInfoLastString = JSON.stringify(this._userInfoLast);
    if (this._lastUser)
        localStorage.setItem(this._storageNamePrefix + "userInfoLast", userInfoLastString);
    else
        localStorage.removeItem(this._storageNamePrefix + "userInfoLast");


    //save isPersistentSignIn
    localStorage.setItem(this._storageNamePrefix + "isPersistentSignIn", this._settings.isPersistentSignIn);

    //save sessionState
    sessionStorage.setItem(this._storageNamePrefix + "sessionState", this._settings.sessionState);
};

directSp.DirectSpClient.prototype.signInByPasswordGrant = function (username, password) {
    var orgIsAuthorized = this.isAuthorized;

    //clear user info and tokens
    if (this.username != username) {
        this._resetUser();
    }

    //save as last user
    this._userInfoLast = {
        username: username
    };

    //create request param
    var requestParam = {
        username: username,
        password: password,
        grant_type: "password",
        scope: this.authScope,
        client_id: this.clientId
    };

    var _this = this;
    return this._ajax(
        {
            url: _this.tokenEndpointUri,
            data: requestParam,
            type: "POST",
            cache: false
        })
        .then(result => {
            return _this.setTokens(result);
        })
        .then(result => {
            _this._fireAuthorizedEvent();
            return result;
        })
        .catch(result => {
            if (orgIsAuthorized)
                _this._fireAuthorizedEvent();
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
        var params = {
            client_id: this.clientId,
            redirect_uri: this.authRedirectUri,
            scope: this.authScope,
            response_type: this.authType,
            state: this._settings.sessionState
        };

        this.isAutoSignIn = false; //let leave the page
        window.location.href = this.logoutEndpointUri + "?" + directSp.Convert.toQueryString(params);
    }

    // always fire AuthorizedEvent
    this._fireAuthorizedEvent();
    return Promise.resolve();
};

directSp.DirectSpClient.prototype.grantAuthorization = function (password) {
    var requestParam = this.authRequest;
    requestParam.SpApp_Authorization = this.authHeader;
    requestParam.permission = "grant";
    if (password != null) requestParam.password = password;
    directSp.Html.submit(this.authEndpointUri + this._originalQueryString, requestParam);
};

directSp.DirectSpClient.prototype.denyAuthorization = function () {
    var requestParam = this.authRequest;
    requestParam.SpApp_Authorization = this.authHeader;
    requestParam.permission = "deny";
    directSp.Html.submit(this.authEndpointUri + this._originalQueryString, requestParam);
};

//navigate to directSp auth server
directSp.DirectSpClient.prototype.signIn = function () {

    //save current location
    sessionStorage.setItem(this._storageNamePrefix + "lastPageUri", window.location.href);

    //redirect to sign in
    var params = {
        client_id: this.clientId,
        redirect_uri: this.authRedirectUri,
        scope: this.authScope,
        response_type: this.authType,
        state: this._settings.sessionState
    };
    window.location.href = this.authEndpointUri + "?" + directSp.Convert.toQueryString(params);
};

//navigate to directSp authorization server
// data will be true if 
directSp.DirectSpClient.prototype._processAuthCallback = function () {
    var _this = this;

    //check is oauth2callback
    if (!this.isAuthCallback)
        return Promise.resolve(false);

    // restore last pageUri
    _this._lastPageUri = sessionStorage.getItem(_this._storageNamePrefix + "lastPageUri");
    if (_this._lastPageUri == null)
        _this._lastPageUri = _this.homePageUri;

    //check state and do nothing if it is not matched
    var state = directSp.Uri.getParameterByName("state");
    if (this._settings.sessionState != state) {
        console.error("DirectSp: Invalid sessionState!");
        this.setTokens(null);
        return Promise.reject(this.createError("Invalid sessionState!"));
    }

    //process authorization_code flow
    var code = directSp.Uri.getParameterByName("code");
    if (code != null) {
        var params = {
            client_id: this.clientId,
            redirect_uri: this.authRedirectUri,
            grant_type: "authorization_code",
            code: code
        };

        return _this._ajax(
            {
                url: _this.tokenEndpointUri,
                data: params,
                type: "POST",
                cache: false
            })
            .then(result => {
                return _this.setTokens(result);
            })
            .catch(error => {
                _this.setTokens(null);
                throw error;
            });
    }

    //process implicit flow
    var access_token = directSp.Uri.getParameterByName("access_token");
    if (access_token != null) {
        return _this.setTokens({
            access_token: access_token,
            token_type: directSp.Uri.getParameterByName("token_type"),
            expires_in: directSp.Uri.getParameterByName("expires_in")
        });
    }

    //finish processAuthCallback without any result
    return Promise.resolve(false);
};

directSp.DirectSpClient.prototype._convertToError = function (data) {

    var error = {};

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
    else if (data.responseJSON != null && (data.responseJSON.errorName != null || data.responseJSON.errorNumber != null || data.responseJSON.error != null)) {
        error = data.responseJSON;
    }
    else if (data.responseText != null) {
        try {
            var responseJSON = JSON.parse(data.responseText);
            if (responseJSON.errorName != null || responseJSON.errorNumber != null || data.responseJSON.error != null) {
                error = responseJSON;

            }
        } catch (e) {
        }
    }
    else {
        error.errorName = "unknown";
        error.errorDescription = "Maybe it was a network error";
        //data will set finally
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
            var obj = JSON.parse(error.errorDescription);
            if (obj.errorName != null || obj.errorNumber != null)
                error = obj;
        } catch (e) {
        }
    }

    //remove undefined
    error.errorType = directSp.Utility.checkUndefined(error.errorType);
    error.errorName = directSp.Utility.checkUndefined(error.errorName);
    error.errorNumber = directSp.Utility.checkUndefined(error.errorNumber);
    error.errorMessage = directSp.Utility.checkUndefined(error.errorMessage);
    error.errorDescription = directSp.Utility.checkUndefined(error.errorDescription);
    error.errorProcName = directSp.Utility.checkUndefined(error.errorProcName);

    error.status = data != null && data.status != null ? data.status : 400;
    error.statusText = data != null && data.statusText != null ? data.statusText : "Bad Request";
    if (error !== data && data != null && error !== data.responseJSON)
        error.innerError = data; //prevent circular object when error is the data
    return error;
};

directSp.DirectSpClient.prototype.invokeBatch = function (spCalls, invokeOptions) {

    var invokeParamsBatch = {
        spCalls: spCalls,
        invokeOptions: invokeOptions
    };

    return this._invokeCore("invokeBatch", invokeParamsBatch, true);
};

//invokeOptions {pagination:"none|client|server", pageSize:10, pageIndex:0}
directSp.DirectSpClient.prototype.invoke = function (method, params, invokeOptions) {

    var spCall = {
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
    var res = this._processPagination(spCall, invokeOptions);
    if (res != null)
        return res;

    // create invokerParams
    var invokeParams = {
        spCall: spCall,
        invokeOptions: invokeOptions
    }

    //call api
    return this._invokeCore(spCall.method, invokeParams, true)
        .then(result => {
            if (invokeOptions.autoDownload)
                window.location = result.recordsetUri;
            return result;
        });
};

// AppErrorHandler
directSp.DirectSpClient.prototype._invokeCore = function (method, invokeParams) {
    var _this = this;

    //set defaults
    if (!invokeParams.invokeOptions) invokeParams.invokeOptions = {};
    invokeParams.invokeOptions.cache == directSp.Convert.toBoolean(invokeParams.invokeOptions.cache, true);
    invokeParams.invokeOptions.isUseAppErrorHandler == directSp.Convert.toBoolean(invokeParams.invokeOptions.isUseAppErrorHandler, this.isUseAppErrorHandler);

    //log request
    if (_this.isLogEnabled)
        console.log("DirectSp: invoke (Request)", invokeParams.spCall, invokeParams.invokeOptions);

    return this._invokeCore2(method, invokeParams)
        .then(result => {
            //log response
            if (_this.isLogEnabled)
                console.log("DirectSp: invoke (Response)", invokeParams.spCall, result);
            return result;
        })
        .catch(error => {
            //call global error handler
            if (invokeParams.invokeOptions.isUseAppErrorHandler && _this.onError) {
                _this.onError(data);
            }
            throw error;
        });
}

directSp.DirectSpClient.prototype._invokeCore2 = function (method, invokeParams) {

    return this._ajax(
        {
            url: directSp.Uri.combine(this.resourceApiUri, method),
            data: JSON.stringify(invokeParams),
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            headers: { 'authorization': this.authHeader },
            cache: invokeParams.invokeOptions.cache,
            timeout: 10 * 60 * 1000 //10 min
        });
};

//manageToken
directSp.DirectSpClient.prototype._ajax = function (ajaxOptions) {

    var _this = this;
    if (!ajaxOptions.headers || !ajaxOptions.authorization)
        return _this._ajax2(ajaxOptions); //there is no token

    return _this._refreshToken()
        .then(result => {
            ajaxOptions.authorization = _this.authHeader;
            return _this._ajax2(ajaxOptions);
        })
        .catch(error => {
            // signout if token is expired
            if (_this._isTokenExpiredError(error) && _this.isAuthorized) {
                return _this.signOut()
                    .then(result => {
                        throw error;
                    });
            }
            throw error;
        });
};

// Handle Captcha
directSp.DirectSpClient.prototype._ajax2 = function (ajaxOptions) {

    var _this = this;
    return this._ajax3(ajaxOptions)
        .catch(error => {
            //log error
            if (_this.isLogEnabled)
                console.error(ajaxOptions.url, ajaxOptions.data, error);

            //check captcha error
            if (_this.onCaptcha) {
                //return if it not a captcha error
                if (!error || !error.errorName || error.errorName.toLowerCase() != "invalidcaptcha")
                    throw error;

                //raise onCaptcha event
                var captchaControllerOptions = {
                    dspClient: _this,
                    ajaxOptions: ajaxOptions,
                    captchaId: data.errorData.captchaId,
                    captchaImage: data.errorData.captchaImage
                }

                var captchaController = new directSp.CaptchaController(captchaControllerOptions);
                if (_this.isLogEnabled) console.log("Calling onCaptcha ...");
                _this.onCaptcha(captchaController);
                return captchaController.promise;
            }

            throw error;
        });
};

//Handle Hook and delay
directSp.DirectSpClient.prototype._ajax3 = function (ajaxOptions) {

    var hookOptions = { delay: 0 };

    // manage hook
    var ajaxPromise = this._processApiHook(ajaxOptions, hookOptions);
    if (ajaxPromise == null)
        ajaxPromise = this._ajaxProvider(ajaxOptions);

    //return the promise if there is no api delay
    if (hookOptions.delay == null || hookOptions.delay <= 0)
        return ajaxPromise;

    //proces delay
    console.warn('DirectSp: Warning! ' + ajaxOptions.url + ' is delayed by ' + delay + ' milliseconds');
    var _this = this;
    return new Promise((resovle, reject) => {
        var interval = hookOptions.delay;
        var delay = directSp.Utility.getRandomInt(interval / 2, interval + interval / 2);
        setTimeout(() => resolve(), delay);
    }).then(result => {
        return ajaxPromise;
    });

};

directSp.DirectSpClient.prototype._ajaxProvider = function (ajaxOptions) {

    //debugger;
    //var fetchOptions = {
    //    body: ajaxOptions.data,
    //    method: 'POST',
    //    //mode: "cors",
    //    headers: {
    //        'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8',
    //        'authorization': this.authHeader,
    //    }
    //};

    return new Promise((resolve, reject) => {
        jQuery.ajax(ajaxOptions)
            .done(data => {
                resolve(data)
            })
            .fail(error => {
                reject(this._convertToError(error))
            });
    });
};

directSp.DirectSpClient.prototype._processApiHook = function (ajaxOptions, hookOptions) {

    //return quickly if there is no hook
    if (!this.apiHook)
        return null;

    // recover invokeParams
    // Note: batch not supported yet
    var invokeParams = null;
    try {
        invokeParams = ajaxOptions.data ? JSON.parse(ajaxOptions.data) : null;
    } catch (err) { }

    // Check is invokeParams available
    if (!invokeParams || !invokeParams.spCall)
        return null;

    var spCall = invokeParams.spCall;
    var invokeOptions = invokeParams.invokeOptions;

    //run hook
    try {
        var data = this.apiHook(spCall.method, spCall.params, hookOptions);
        if (data == null)
            return null;

        //clone data
        data = directSp.Utility.clone(data);

        //support paging
        if (spCall != null && spCall.params != null && invokeOptions.recordIndex != null && data.recordset != null) {
            if (invokeOptions.recordCount == null) invokeOptions.recordCount = 20;
            data.recordset = data.recordset.slice(invokeOptions.recordIndex, invokeOptions.recordIndex + invokeOptions.recordCount);
        }

        return Promise.resolve(data);
    }
    catch (e) {
        return Promise.reject(this._convertToError(e)); //make sure hook error converted to SpError
    }
}

directSp.DirectSpClient.prototype.help = function (criteria) {
    //Load apiMetadata
    if (this._systemApi == null) {
        var _this = this;
        _this.invoke("System_Api")
            .then(result => {
                _this._systemApi = result.apiMetadata;
                _this.help(criteria);
                return result;
            });
        return 'wait...';
    }

    // show help
    if (criteria != null) criteria = criteria.trim().toLowerCase();
    if (criteria == "") criteria = null;

    //find all proc that match
    var foundProc = [];
    for (var i = 0; i < this._systemApi.length; i++) {
        if (criteria == null || this._systemApi[i].procedureName.toLowerCase().indexOf(criteria) != -1) {
            foundProc.push(this._systemApi[i]);
        }
    }

    //show procedure if there is only one procedure
    if (foundProc.length == 0) {
        console.log('Nothing found!');
    }
    else {
        for (i = 0; i < foundProc.length; i++) {
            if (foundProc[i].procedureName.toLowerCase() == criteria || foundProc.length == 1)
                this._help(foundProc[i]);
            else
                console.log(foundProc[i].procedureName);
        }
    }

    return '---------------';
};

directSp.DirectSpClient.prototype._help = function (procedureMetadata) {
    // find max param length
    var maxParamNameLength = 0;
    var inputParams = [];
    for (var i = 0; i < procedureMetadata.params.length; i++) {
        var param = procedureMetadata.params[i];
        maxParamNameLength = Math.max(maxParamNameLength, param.paramName.length)
        if (!param.isOutput && param.paramName.toLowerCase() != '@context') {
            inputParams.push(this._formatHelpParamName(param.paramName));
        }
    }
    var s = {
        a: "",
        b: {}
    }
    //prepare input params
    var str = "";
    str += "\n" + '---------------';
    str += "\n" + "Method:";
    str += "\n\t" + procedureMetadata.procedureName + ' (' + inputParams.join(", ") + ')';
    str += "\n";
    str += "\n" + "Parameters:";
    for (i = 0; i < procedureMetadata.params.length; i++) {
        var param = procedureMetadata.params[i];
        if (!param.isOutput && param.paramName.toLowerCase() != '@context') {
            str += "\n\t" + this._getHelpParam(procedureMetadata, param, maxParamNameLength);
        }
    }

    //prepare ouput params
    str += "\n";
    str += "\n" + "Returns:";
    for (i = 0; i < procedureMetadata.params.length; i++) {
        var param = procedureMetadata.params[i];
        if (param.isOutput && param.paramName.toLowerCase() != '@context') {
            str += "\n\t" + this._getHelpParam(procedureMetadata, param, maxParamNameLength);
        }
    }
    str += "\n\t" + this._formatHelpParam("returnValue", "integer", maxParamNameLength);
    str += "\n\t" + this._formatHelpParam("recordset", "array", maxParamNameLength);

    //sample
    var sample = 'dspClient.invoke("$(procname)", { $(parameters) })';
    var sampleParam = [];
    for (i = 0; i < inputParams.length; i++)
        sampleParam.push(inputParams[i] + ': ' + '$' + inputParams[i]);
    sample = sample.replace('$(procname)', procedureMetadata.procedureName);
    sample = sample.replace('$(parameters)', sampleParam.join(", "));
    str += "\n";
    str += "\n" + "Sample:";
    str += "\n\t" + sample;

    console.log(str)
}

directSp.DirectSpClient.prototype._getHelpParam = function (procedureMetadata, param, maxParamNameLength) {
    var paramType = this._getHelpParamType(procedureMetadata, param);
    return this._formatHelpParam(param.paramName, paramType, maxParamNameLength);
}

directSp.DirectSpClient.prototype._getHelpParamType = function (procedureMetadata, param) {

    //check userTypeName
    var userTypeName = param.userTypeName != null ? param.userTypeName.toLowerCase() : "";
    if (userTypeName.indexOf('json') != -1)
        return 'object';

    //check systemTypeName
    var paramType = param.systemTypeName.toLowerCase();
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
    var str = this._formatHelpParamName(paramName);

    //add spaces
    for (var i = str.length; i < maxParamNameLength + 2; i++)
        str += ' ';

    return str + "(" + paramType + ")";
};

directSp.DirectSpClient.prototype._processPagination = function (spCall, invokeOptions) {

    //prevent recursive call
    if (invokeOptions.pagination == null && invokeOptions.pageSize != null) invokeOptions.pagination = "server";
    if (invokeOptions.pagination == null || invokeOptions.pagination == "" || invokeOptions.pagination == "none")
        return null;

    //create paginator
    var paginator = new directSp.DirectSpClient.Paginator(this, spCall, invokeOptions);
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
        return (this._pageCountMax == null || (this.pageIndex + 1) < this._pageCountMax);
    },

    get hasPrevPage() {
        return this.pageIndex > 1;
    },

    get recordset() {
        var ret = this._pages[this.pageIndex];
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
        return this._pageCountMin;
    },

    get pageCountMax() {
        return this._pageCountMax;
    },

    get pageCount() {
        return this._pageCount;
    },

    get recordCount() {
        if (this.pageCount == null)
            return null;

        if (this.pageCount == 0 || this._pages.length == 0)
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
    var newInvokeOptions = {
        recordsetFileTitle: this._invokeOptions.recordsetFileTitle,
        autoDownload: true
    };

    var promise = this._dspClient.invoke2(this._apiCall, newInvokeOptions);
    return promise;
},

    directSp.DirectSpClient.Paginator.prototype.getApproxPageCount = function (maxPageCount) {
        var value = Math.max(maxPageCount, this.pageCountMin);
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
    var curPageIndex = this.pageIndex;
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
};

directSp.DirectSpClient.Paginator.prototype.goPage = function (pageNo) {
    var _this = this;
    this._isCacheInvalidated = false;
    this._isCacheUsed = true;
    this._isInvoked = false;
    if (!pageNo) pageNo = 0;

    //validate pageNo
    if (pageNo == -1) pageNo = 0;
    if (this.pageCountMax != null) pageNo = Math.min(pageNo, this.pageCountMax - 1);

    //find page range
    var pageStart = Math.max(0, pageNo - this._pageCacheCount);
    var pageEnd = pageNo + this._pageCacheCount;

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
    var pageCount = pageEnd - pageStart + 1;

    if (pageCount > 0) {

        //calculate recourdIndex and recordCount
        var recordIndex = pageStart * this.pageSize;
        var recordCount = pageCount * this.pageSize + 1; //additional records to find last page
        if (pageStart > 0) {
            recordIndex--;
            recordCount++;
        }

        //remove pagination invokeOptions
        var invokeOptions = directSp.Utility.clone(this._invokeOptions);
        invokeOptions.pagination = "none";
        invokeOptions.recordIndex = recordIndex;
        invokeOptions.recordCount = recordCount;
        if (this._invokeOptions.pagination == "client") {
            invokeOptions.recordIndex = 0;
            invokeOptions.recordCount = -2;
        }

        //invoke
        this._isInvoked = true;
        var promise = this._dspClient.invoke2(this._apiCall, invokeOptions)
            .then(result => {
                var recordset = result.recordset != null ? result.recordset : [];

                //Detect record shift on left; Clear caches if the first record is not matched to last record of the previous page
                var prePageRecordset = pageStart > 0 ? _this._pages[pageStart - 1] : null;
                if (prePageRecordset != null && (recordset.length == 0 || JSON.stringify(prePageRecordset[_this.pageSize - 1]) != JSON.stringify(recordset[0])))
                    _this.reset();

                //Detect record shift on right; Clear cache if the last record is not matched to first record of the previous page
                var nextPageRecordset = (pageEnd + 1 < _this._pages.length) ? _this._pages[pageEnd + 1] : null;
                if (nextPageRecordset != null && (recordset.length < recordCount || nextPageRecordset[0] != recordset[recordset.length - 1]))
                    _this.reset();

                //estimate pageCount
                var lastRecordsetPageIndex = Math.floor((recordIndex + recordset.length) / _this.pageSize);
                if ((recordIndex + recordset.length) % _this.pageSize == 0) lastRecordsetPageIndex--;

                if (recordset.length == 0) {
                    if (pageStart <= _this.pageCountMin) {
                        _this.reset();
                    }

                    if (pageStart <= 1) {
                        _this._pageCount = 1;
                        _this._pageCountMax = 1;
                    }

                    _this._pageCountMax = Math.min(_this._pageCountMax == null ? lastRecordsetPageIndex + 1 : _this._pageCountMax, lastRecordsetPageIndex + 1);
                }
                else {
                    if (_this._pageCountMax != null && pageStart >= _this._pageCountMax) {
                        _this.reset();
                    }

                    if (recordset.length != recordCount) {
                        _this._pageCount = lastRecordsetPageIndex + 1;
                        _this._pageCountMin = lastRecordsetPageIndex + 1;
                        _this._pageCountMax = lastRecordsetPageIndex + 1;
                    }
                    else {
                        _this._pageCountMin = Math.max(_this._pageCountMin, lastRecordsetPageIndex + 1);
                    }
                }

                //assign pages
                var i = recordIndex + (recordIndex % _this.pageSize) != 0 ? _this.pageSize - (recordIndex % _this.pageSize) : 0;
                for (; i < recordset.length; i = i + _this.pageSize) {
                    var pageIndex = Math.floor((recordIndex + i) / _this.pageSize);
                    var pageRecordset = recordset.slice(i, i + _this.pageSize);

                    if (pageRecordset.length > 0 && (pageRecordset.length == _this.pageSize || recordset.length < recordCount)) {
                        _this._pages[pageIndex] = pageRecordset;
                        _this._pagePromises[pageIndex] = promise;
                        if (pageNo == pageIndex) _this._isCacheUsed = false;
                    }
                }

                return result;
            });

        //assign promises
        for (var i = pageStart; i <= pageEnd; i++)
            this._pagePromises[i] = promise;
    }

    //change current page after getting result
    return this._pagePromises[pageNo]
        .then(result => {
            _this._pageIndex = pageNo;
            return result;
        });
};

// *********************
// **** Captcha Controller
// *********************
directSp.CaptchaController = function (data) {
    this._data = data;
    this.captchaImageUri = "data:image/png;base64," + data.captchaImage;
    var _this = this;
    this.promise = new Promise((resolve, reject) => {
        _this._resolve = resolve;
        _this._reject = reject;
    });
};

directSp.CaptchaController.prototype.continue = function (captchaCode) {
    var ajaxOptions = this._data.ajaxOptions;

    //finding ajax data 
    var ajaxData = ajaxOptions.data;

    //try parse ajax data as json
    try {
        ajaxData = JSON.parse(ajaxOptions.data);
    } catch (e) { }

    // try get invokeOptions
    var invokeOptions = null;
    if (ajaxData)
        invokeOptions = ajaxData.invokeOptions;

    //try update invokeParams for invoke
    if (invokeOptions) {
        invokeOptions.captchaId = this._data.captchaId;
        invokeOptions.captchaCode = captchaCode;
        ajaxOptions.data = JSON.stringify(ajaxData)
    }

    //try update param for authentication grantby password
    if (ajaxData.grant_type == "password") {
        ajaxData.captchaId = this._data.captchaId;
        ajaxData.captchaCode = captchaCode;
    }

    // retry original ajax
    var _this = this;
    this._data.dspClient._ajax(ajaxOptions)
        .then(data => {
            return _this._resolve(data);
        })
        .catch(error => {
            return _this._reject(error);
        });
}

directSp.CaptchaController.prototype.cancel = function () {
    var error = this._data.dspClient._convertToError("Captcha has been canceled by the user!");
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
    var base64Url = token.split('.')[1];
    var base64 = base64Url.replace('-', '+').replace('_', '/');
    return JSON.parse(window.atob(base64));
};

directSp.Utility.clone = function (obj) {
    if (obj == null)
        return obj;
    return JSON.parse(JSON.stringify(obj));
};

directSp.Utility.shallowCopy = function (src, des) {
    for (var item in src) {
        if (src.hasOwnProperty(item))
            des[item] = src[item];
    }
};

directSp.Utility.deepCopy = function (src, des) {
    for (var item in src) {
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

//Utilities
directSp.Convert = {};
directSp.Convert.toBoolean = function (value, defaultValue) {
    defaultValue = directSp.Utility.checkUndefined(defaultValue, false);
    try {
        var parsed = parseInt(value);
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
        var ret = parseInt(value);
        if (!isNaN(ret))
            return ret;
    }
    catch (err) {
    }
    return defaultValue;
};

directSp.Convert.toQueryString = function (obj) {
    var parts = [];
    for (var i in obj) {
        if (obj.hasOwnProperty(i)) {
            parts.push(encodeURIComponent(i) + "=" + encodeURIComponent(obj[i]));
        }
    }
    return parts.join("&");
};

//Utilities
directSp.Uri = {};

directSp.Uri.getParameterByName = function (name, url) {
    if (!url) url = window.location.href;

    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[#?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
};

directSp.Uri.combine = function (uriBase, uriRelative) {

    var parts = [uriBase, uriRelative];
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
    var endIndex = uri.indexOf("?");
    if (endIndex == -1) endIndex = uri.length;
    var lastSlash = uri.lastIndexOf("/", endIndex);
    return uri.substr(0, lastSlash);
};

directSp.Uri.getUrlWithoutQueryString = function (uri) {
    return uri.split("?").shift().split("#").shift();
};

directSp.Uri.getFileName = function (uri) {
    uri = directSp.Uri.getUrlWithoutQueryString(uri);
    return uri.split("/").pop();
};

// html
directSp.Html = {};
directSp.Html.submit = function (url, params) {
    var method = "post";

    var form = document.createElement("form");
    form.setAttribute("method", method);
    form.setAttribute("action", url);

    for (var key in params) {
        if (params.hasOwnProperty(key)) {
            var hiddenField = document.createElement("input");
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
var dspClient = new directSp.DirectSpClient();
