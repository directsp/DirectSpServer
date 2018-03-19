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

//SpApp
directSp.SpApp = function () {
    this._settings = {
        client_id: "",
        homePageUri: window.location.origin,
        authRedirectUri: window.location.origin + "/oauth2/callback",
        authUri: "https://auth.directsp.net",
        authScope: "offline_access", //openid offline_access profile phone email address
        authType: "token", //token, code
        resourceApiUri: null,
        isPersistentSignIn: true,
        isAutoSignIn: false,
        isLogEnabled: true,
        isUseAppErrorHandler: false,
        sessionState: Math.floor(Math.random() * 10000000000000)
    };

    this._auth = {
        tokens: null, //{ access_token: '', expires_in: 0, refresh_token: '', token_type: '' },
        authEndpointPath: "/connect/authorize",
        tokenEndpointPath: "/connect/token",
        userinfoEndpointPath: "/connect/userinfo",
        logoutEndpointPath: "/connect/logout"
    };

    this._authRequest = {
        client_id: directSp.Uri.getParameterByName('client_id'),
        redirect_uri: directSp.Uri.getParameterByName('redirect_uri'),
        scope: directSp.Uri.getParameterByName('scope'),
        response_type: directSp.Uri.getParameterByName('response_type'),
        state: directSp.Uri.getParameterByName('state')
    };

    this._lastUser = {
        user: null,
        userDisplayName: null
    };

    this._isInitializing = true;
    this._isLogoutInProgress = false;
    this._lastPageUri = null;
    this._apiHook = function (method, params) { return null; }
    this._authError = null;
    this._user = null;
    this._onAuthorized = null;
    this._onCaptcha = null;
    this._onError = null;
    this._storageNamePrefix = "SpApp:";
    this._originalUri = location.href;
    this._originalQueryString = location.search;
    this._apiMetadata = null;
    this._load();
};

directSp.SpApp.prototype = {
    get user() {
        return this._user;
    },

    get username() {
        //get username from user in app (except password grant)
        if (this.user)
            return this.user.username ? this.user.username : null; //prevent undefined

        // get last username for signin
        return this._lastUser ? this._lastUser.username : null;
    },

    get userId() {
        return this.user != null ? this.user.sub : null;
    },

    get userDisplayName() {
        if (this.user)
            return this.user.name ? this.user.name : this.username;

        //try last displayName
        if (this._lastUser)
            return this._lastUser.userDisplayName ? this._lastUser.userDisplayName : this._lastUser.username;

        return null;
    },

    get client_id() {
        return this._settings.client_id;
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
        return this._auth.tokens !== null;
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
    get authUri() {
        return this._settings.authUri;
    },
    set authUri(value) {
        this._settings.authUri = value;
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
        return this._auth.tokens;
    },
    set tokens(value) {
        if (value == this._auth.tokens)
            return; //no change

        //set token
        this._auth.tokens = value;

        //update auth data
        if (value != null) {
            //set firstName and lastName from tokens
            var jwt = directSp.Utility.parseJwt(this.tokens.access_token);
            if (this._user == null) this._user = {};
            this._user.sub = jwt.sub;
        }
        else {
            this._user = null;
            this._fireAuthorizedEvent();
        }

        this._save();
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
    get authEndpointUri() {
        return directSp.Uri.combine(this.authUri, this._auth.authEndpointPath);
    },
    get tokenEndpointUri() {
        return directSp.Uri.combine(this.authUri, this._auth.tokenEndpointPath);
    },
    get userinfoEndpointUri() {
        return directSp.Uri.combine(this.authUri, this._auth.userinfoEndpointPath);
    },
    get logoutEndpointUri() {
        return directSp.Uri.combine(this.authUri, this._auth.logoutEndpointPath);
    },
    get authHeader() {
        return this.tokens != null ? this.tokens.token_type + ' ' + this.tokens.access_token : null;
    },
    get authRequest() {
        return this._authRequest;
    },
    get authRequestUri() {
        return window.location.origin + "?" + jQuery.param(this._authRequest);
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
directSp.SpApp.prototype.init = function (client_id) {
    console.log("spApp: initializing ...");
    var deferred = new jQuery.Deferred();
    var _this = this;
    this._isInitializing = true;
    this._settings.client_id = client_id;

    //process error
    if (directSp.Uri.getParameterByName("error") != null) {
        this._authError = this._convertToError({
            error: directSp.Uri.getParameterByName("error"),
            error_description: directSp.Uri.getParameterByName("error_description")
        });

        console.error("spApp: Auth Error!", this._authError);
        if (this.isAutoSignIn) {
            this.isAutoSignIn = false;
            console.warn("spApp: isAutoSignIn is set to false due Auth Error", this.authError);
        }
    }

    this._processAuthCallback()
        .always(function () {
            //check for auth after callback processing
            _this.checkAuth()
                .always(function () {
                    _this._isInitializing = false;
                    _this._fireAuthorizedEvent();
                });
            deferred.resolve(true);
        });

    return deferred.promise();
};

directSp.SpApp.prototype.createError = function (error) {
    return this._convertToError(error); //fix error
};

directSp.SpApp.prototype.throwAppError = function (error) {
    if (this._onError)
        this._onError(this._convertToError(error)); //fix error
};


directSp.SpApp.prototype._fireAuthorizedEvent = function () {
    var _this = this;

    setTimeout(function () {

        //don't fire in initializing state
        if (_this._isInitializing || _this._isLogoutInProgress)
            return;

        //fire the event
        if (_this.isAuthorized)
            console.log("spApp: User has been authorized", _this.user);
        else
            console.log("spApp: User has not been authorized!");

        if (!_this.isAuthorized && _this.isAutoSignIn) {
            _this.signIn();
            return;
        }

        //trigger the even
        if (_this.onAuthorized != null) {
            var data = { lastPageUri: _this._lastPageUri }
            _this.onAuthorized(data);
        }

        _this._lastPageUri = null; //reset last page notifying user
        sessionStorage.removeItem(_this._storageNamePrefix + "lastPageUri");

    }, 0);
};

directSp.SpApp.prototype._resetUser = function () {
    this._auth.tokens = null;
    this._user = null;
    this._lastUser = null;
    this._save();
};

directSp.SpApp.prototype._isTokenExpired = function (data) {
    if (data == null)
        return false;

    //check database AccessDeniedOrObjectNotExists error; it means token has been validated
    if (data.errorName == "AccessDeniedOrObjectNotExists")
        return false;

    //noinspection JSUnresolvedVariable
    if (data.errorName == "invalid_grant")
        return true;

    return data.status == 401 || (data.statusText != null && data.statusText.toLowerCase() == "unauthorized");
};

//check auth from server. Refresh Token if possible
directSp.SpApp.prototype.checkAuth = function () {
    //try to updateUserInfo for authorized users
    if (this.isAuthorized)
        return this._updateUserInfo();

    //reject for unauthrized users
    var deferred = new jQuery.Deferred();
    deferred.reject();
    return deferred;
};

directSp.SpApp.prototype._updateUserInfo = function () {
    var deferred = new jQuery.Deferred();

    var _this = this;
    this.getUserInfo(true)
        .done(function (data) {
            _this._user = data;
            _this._lastUser = {
                username: _this.username,
                userDisplayName: _this.userDisplayName
            }
            deferred.resolve();
        })
        .fail(function (data) {
            deferred.reject(data);
        });

    return deferred.promise();
};

directSp.SpApp.prototype.getUserInfo = function (tryRefreshToken) {
    var deferred = new jQuery.Deferred();
    tryRefreshToken = directSp.Utility.checkUndefined(tryRefreshToken, true);

    //return false if token not exists
    if (!this.isAuthorized) {
        var data = { errorName: "unauthorized", errorMessage: "Can not refresh token for unauthorized users" };
        deferred.reject(this._convertToError(data));
        return deferred.promise();
    }

    //call isAuthorized of server
    var _this = this;
    this._ajax(
        {
            url: this.userinfoEndpointUri,
            data: null,
            type: "GET",
            headers: { 'authorization': this.authHeader },
            cache: false
        })
        .done(function (data) {
            if (_this.isLogEnabled) console.log("spApp: userInfo", data);
            deferred.resolve(data);
        })
        .fail(function (data) {
            if (_this._isTokenExpired(data) && tryRefreshToken) {
                _this._refreshToken()
                    .done(function () {
                        _this.getUserInfo(false)
                            .done(function (data) {
                                deferred.resolve(data);
                            })
                            .fail(function (data) {
                                deferred.reject(data);
                            });
                    })
                    .fail(function () {
                        deferred.reject(data);
                    });
            }
            else {
                deferred.reject(data);
            }
        });

    return deferred.promise();
};

directSp.SpApp.prototype._refreshToken = function () {
    var deferred = new jQuery.Deferred();

    //return false if token not exists
    if (this.tokens == null || this.tokens.refresh_token == null) {
        this.tokens = null;
        deferred.reject();
        return deferred.promise();
    }

    //Refreshing token
    console.log("spApp: Refreshing current token ...");

    //create request param
    var requestParam = {
        grant_type: "refresh_token",
        refresh_token: this.tokens.refresh_token,
        client_id: this.client_id
    };

    //call 
    var _this = this;
    this._ajax(
        {
            url: _this.tokenEndpointUri,
            data: requestParam,
            type: "POST",
            cache: false
        })
        .done(function (data) {
            _this.tokens = data;
            deferred.resolve(data);
        })
        .fail(function (data) {
            _this.tokens = null;
            deferred.reject(data);
        });

    return deferred.promise();
};

directSp.SpApp.prototype._load = function () {
    try {
        //restore tokens
        var tokensString = sessionStorage.getItem(this._storageNamePrefix + "auth.tokens");
        if (!tokensString)
            tokensString = localStorage.getItem(this._storageNamePrefix + "auth.tokens");
        if (tokensString)
            this._auth.tokens = JSON.parse(tokensString);

        //restore user
        var userString = sessionStorage.getItem(this._storageNamePrefix + "user");
        if (!userString)
            userString = localStorage.getItem(this._storageNamePrefix + "user");
        if (userString)
            this._user = JSON.parse(userString);

        //save lastUser
        var lastUserString = localStorage.getItem(this._storageNamePrefix + "lastUser");
        if (lastUserString)
            this._user = JSON.parse(lastUserString);

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

directSp.SpApp.prototype._save = function () {

    //save tokens
    var tokenString = JSON.stringify(this._auth.tokens);
    sessionStorage.setItem(this._storageNamePrefix + "auth.tokens", tokenString);
    if (this.isPersistentSignIn)
        localStorage.setItem(this._storageNamePrefix + "auth.tokens", tokenString);
    else
        localStorage.removeItem(this._storageNamePrefix + "auth.tokens");

    //save user
    var userString = JSON.stringify(this._user);
    sessionStorage.setItem(this._storageNamePrefix + "user", userString);
    if (this.isPersistentSignIn)
        localStorage.setItem(this._storageNamePrefix + "user", userString);
    else
        localStorage.removeItem(this._storageNamePrefix + "user");

    //save lastUser login
    var lastUserString = JSON.stringify(this._lastUser);
    if (this._lastUser)
        localStorage.setItem(this._storageNamePrefix + "lastUser", lastUserString);
    else
        localStorage.removeItem(this._storageNamePrefix + "lastUser");


    //save isPersistentSignIn
    localStorage.setItem(this._storageNamePrefix + "isPersistentSignIn", this._settings.isPersistentSignIn);

    //save sessionState
    sessionStorage.setItem(this._storageNamePrefix + "sessionState", this._settings.sessionState);
};

directSp.SpApp.prototype.signInByPasswordGrant = function (username, password) {

    //clear user info and tokens
    if (this.username != username) {
        this._resetUser();
    }

    //save as last user
    this._lastUser = {
        username: username
    };

    //create request param
    var requestParam = {
        grant_type: "password",
        username: username,
        password: password,
        scope: this.authScope,
        client_id: this.client_id,
    };

    var deferred = new jQuery.Deferred();
    var _this = this;
    this._ajax(
        {
            url: _this.tokenEndpointUri,
            data: requestParam,
            type: "POST",
            cache: false
        })
        .done(function (data) {
            _this.tokens = data;
            _this._updateUserInfo().always(function () {
                _this._fireAuthorizedEvent();
                deferred.resolve();
            });
        })
        .fail(function (data) {
            _this.tokens = null;
            deferred.reject(data);
        });

    return deferred.promise();
};

//signOut but keep current username
directSp.SpApp.prototype.signOut = function (clearUser, redirect) {
    // set default
    redirect = directSp.Utility.checkUndefined(redirect, true);

    // clear access_tokens
    this.tokens = null;

    if (clearUser)
        this._resetUser();

    //redirect to signout page
    if (redirect) {
        var params = {
            client_id: this.client_id,
            redirect_uri: this.authRedirectUri,
            scope: this.authScope,
            response_type: this.authType,
            state: this._settings.sessionState
        };

        this._isLogoutInProgress = true;
        window.location.href = this.logoutEndpointUri + "?" + jQuery.param(params);
    }

    var deferred = jQuery.Deferred();
    deferred.resolve();
    return deferred.promise();
};

directSp.SpApp.prototype.grantAuthorization = function (password) {
    var requestParam = this.authRequest;
    requestParam.SpApp_Authorization = this.authHeader;
    requestParam.permission = "grant";
    if (password != null) requestParam.password = password;
    directSp.Html.submit(this.authEndpointUri + this._originalQueryString, requestParam);
};

directSp.SpApp.prototype.denyAuthorization = function () {
    var requestParam = this.authRequest;
    requestParam.SpApp_Authorization = this.authHeader;
    requestParam.permission = "deny";
    directSp.Html.submit(this.authEndpointUri + this._originalQueryString, requestParam);
};

//navigate to directSp auth server
directSp.SpApp.prototype.signIn = function () {

    //save current location
    sessionStorage.setItem(this._storageNamePrefix + "lastPageUri", window.location.href);

    //redirect to sign in
    var params = {
        client_id: this.client_id,
        redirect_uri: this.authRedirectUri,
        scope: this.authScope,
        response_type: this.authType,
        state: this._settings.sessionState
    };
    window.location.href = this.authEndpointUri + "?" + jQuery.param(params);

    var deferred = jQuery.Deferred();
    deferred.resolve();
    return deferred.promise();
};

//navigate to directSp authorization server
// data will be true if 
directSp.SpApp.prototype._processAuthCallback = function () {
    var deferred = jQuery.Deferred();
    var _this = this;

    //check is oauth2callback
    if (!this.isAuthCallback) {
        deferred.resolve();
        return deferred.promise();
    }

    // restore last pageUri
    _this._lastPageUri = sessionStorage.getItem(_this._storageNamePrefix + "lastPageUri");
    if (_this._lastPageUri == null)
        _this._lastPageUri = _this.homePageUri;

    //check state and do nothing if it is not matched
    var state = directSp.Uri.getParameterByName("state");
    if (this._settings.sessionState != state) {
        console.error("spApp: Invalid sessionState!");
        this.tokens = null;
        deferred.resolve();
        return deferred.promise();
    }

    //process authorization_code flow
    var code = directSp.Uri.getParameterByName("code");
    if (code != null) {
        var params = {
            client_id: this.client_id,
            redirect_uri: this.authRedirectUri,
            grant_type: "authorization_code",
            code: code
        };

        _this._ajax(
            {
                url: _this.tokenEndpointUri,
                data: params,
                type: "POST",
                cache: false
            })
            .done(function (data) {
                _this.tokens = data;
                _this._updateUserInfo().always(function () {
                    deferred.resolve();
                });
            })
            .fail(function (data) {
                console.warn(data);
                _this.tokens = null;
                deferred.resolve();
            });
    }

    //process implicit flow
    var access_token = directSp.Uri.getParameterByName("access_token");
    if (access_token != null) {
        this.tokens = {
            access_token: access_token,
            token_type: directSp.Uri.getParameterByName("token_type"),
            expires_in: directSp.Uri.getParameterByName("expires_in")
        };
        deferred.resolve();
    }

    return deferred.promise();
};

directSp.SpApp.prototype._convertToError = function (data) {

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

directSp.SpApp.prototype.invokeApiBatch = function (spCalls, invokeOptions) {

    var invokeParamsBatch = {
        spCalls: spCalls,
        invokeOptions: invokeOptions
    };

    return this._invokeApiCore("invokeBatch", invokeParamsBatch, true);
};

//invokeOptions {pagination:"none|client|server", pageSize:10, pageIndex:0}
directSp.SpApp.prototype.invokeApi = function (method, params, invokeOptions) {

    var spCall = {
        method: method,
        params: params
    };

    return this.invokeApi2(spCall, invokeOptions);
};

directSp.SpApp.prototype.invokeApi2 = function (spCall, invokeOptions) {
    //validate
    if (spCall == null) throw "spCall is expected";
    if (spCall.method == null) throw "method is expected";
    if (spCall.params == null) spCall.params = {};
    if (this.resourceApiUri == null) throw "spApp.resourceApiUri is not set";

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
    return this._invokeApiCore(spCall.method, invokeParams, true)
        .done(function (data) {
            if (invokeOptions.autoDownload == 1)
                window.location = data.recordsetUri;
        });
};

// Pipe error
directSp.SpApp.prototype._invokeApiCore = function (method, invokeParams) {

    //set defaults
    if (invokeParams.invokeOptions == null) invokeParams.invokeOptions = {};
    if (invokeParams.invokeOptions.cache == null) invokeParams.invokeOptions.cache = true;
    if (invokeParams.invokeOptions.cache == null) invokeParams.invokeOptions.cache = true;
    if (invokeParams.invokeOptions.isUseAppErrorHandler == null) invokeParams.invokeOptions.isUseAppErrorHandler = this.isUseAppErrorHandler;

    var _this = this;
    return this._invokeApiCore2(method, invokeParams, true).fail(function (data) {
        if (invokeParams.invokeOptions.isUseAppErrorHandler && _this.onError) {
            _this.onError(data);
        }
    });
}

directSp.SpApp.prototype._invokeApiCore2 = function (method, invokeParams, tryRefreshToken) {

    //log request
    if (this.isLogEnabled) console.log("spApp: invokeApi (Request)", invokeParams);

    //run ajax
    var _this = this;
    var deferred = new jQuery.Deferred();
    this._ajax(
        {
            url: directSp.Uri.combine(_this.resourceApiUri, method),
            data: JSON.stringify(invokeParams),
            dataType: "json",
            type: "POST",
            contentType: "application/json; charset=utf-8",
            headers: { 'authorization': this.authHeader },
            cache: invokeParams.invokeOptions.cache,
            timeout: 10 * 60 * 1000 //10 min
        })
        .done(function (data) {
            if (_this.isLogEnabled) console.log("spApp: invokeApi (Response)", invokeParams, data);
            deferred.resolve(data);
        })
        .fail(function (data) {
            if (_this._isTokenExpired(data) && tryRefreshToken) {
                _this._refreshToken()
                    .done(function () {
                        _this._updateUserInfo();
                        _this._invokeApiCore2(method, invokeParams, false)
                            .done(function (data) {
                                deferred.resolve(data);
                            })
                            .fail(function (data) {
                                deferred.reject(data);
                            });
                    })
                    .fail(function () {
                        deferred.reject(data);
                    });
            }
            else {
                deferred.reject(data);
            }
        });

    return deferred.promise();
};

directSp.SpApp.prototype.help = function (criteria) {
    //Load apiMetadata
    if (this._apiMetadata == null) {
        var _this = this;
        this.invokeApi("System_ApiMetadata")
            .done(function (data) {
                _this._apiMetadata = data.apiMetadata;
                _this.help(criteria);
            });
        return 'wait...';
    }

    // show help
    if (criteria != null) criteria = criteria.trim().toLowerCase();
    if (criteria == "") criteria = null;

    //find all proc that match
    var foundProc = [];
    for (var i = 0; i < this._apiMetadata.length; i++) {
        if (criteria == null || this._apiMetadata[i].procedureName.toLowerCase().indexOf(criteria) != -1) {
            foundProc.push(this._apiMetadata[i]);
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

directSp.SpApp.prototype._help = function (procedureMetadata) {
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
    var sample = 'spApp.invokeApi("$(procname)", { $(parameters) })';
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

directSp.SpApp.prototype._getHelpParam = function (procedureMetadata, param, maxParamNameLength) {
    var paramType = this._getHelpParamType(procedureMetadata, param);
    return this._formatHelpParam(param.paramName, paramType, maxParamNameLength);
}

directSp.SpApp.prototype._getHelpParamType = function (procedureMetadata, param) {

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

directSp.SpApp.prototype._formatHelpParamName = function (paramName) {
    //remove extra characters
    if (paramName.length > 0 && paramName[0] == '@')
        paramName = paramName.substr(1);
    return directSp.Utility.toCamelcase(paramName);
};

directSp.SpApp.prototype._formatHelpParam = function (paramName, paramType, maxParamNameLength) {
    var str = this._formatHelpParamName(paramName);

    //add spaces
    for (var i = str.length; i < maxParamNameLength + 2; i++)
        str += ' ';

    return str + "(" + paramType + ")";
};

// Start piping
directSp.SpApp.prototype._ajax = function (ajaxOptions) {
    return this._ajaxHelper(ajaxOptions);
};

// Handle Captcha
directSp.SpApp.prototype._ajaxHelper = function (ajaxOptions) {

    //bypass if onCaptcha is not overrided
    if (!this.onCaptcha)
        return this._ajaxHelper2(ajaxOptions);

    // create captcha deferred
    var _this = this;
    var deferred = new jQuery.Deferred();

    this._ajaxHelper2(ajaxOptions)
        .done(function (data) {
            deferred.resolve(data);
        })
        .fail(function (data) {
            //return if it not a captcha error
            if (!data || !data.errorName || data.errorName.toLowerCase() != "invalidcaptcha") {
                deferred.reject(data);
                return;
            }

            //raise onCaptcha event
            var captchaControllerOptions = {
                spApp: _this,
                ajaxOptions: ajaxOptions,
                deferred: deferred,
                captchaId: data.errorData.captchaId,
                captchaImage: data.errorData.captchaImage
            }

            var captchaController = new directSp.SpApp.CaptchaController(captchaControllerOptions);
            if (_this.isLogEnabled) console.log("Calling onCaptcha ...");
            _this.onCaptcha(captchaController);
        });

    return deferred.promise();
};

// handle error
directSp.SpApp.prototype._ajaxHelper2 = function (ajaxOptions) {
    var _this = this;
    return this._ajaxHelper3(ajaxOptions).pipe(null, function (data) {
        var error = _this._convertToError(data);
        if (_this.isLogEnabled) {
            console.error(ajaxOptions.url, ajaxOptions.data, error);
        }
        return error;
    });
}

//Handle Hook and delay
directSp.SpApp.prototype._ajaxHelper3 = function (ajaxOptions) {

    var hookOptions = { delay: 0 };

    // manage hook
    var ajaxPromise = this._processApiHook(ajaxOptions, hookOptions);
    if (ajaxPromise == null)
        ajaxPromise = this._ajaxProvider(ajaxOptions);

    //return the promise if there is no api delay
    if (hookOptions.delay == null || hookOptions.delay <= 0)
        return ajaxPromise;

    //proces delay
    var _this = this;
    var deterred = new jQuery.Deferred();
    var interval = hookOptions.delay;
    var delay = directSp.Utility.getRandomInt(interval / 2, interval + interval / 2);
    console.warn('spApp: Warning! ' + ajaxOptions.url + ' is delayed by ' + delay + ' milliseconds');

    setTimeout(function () {
        ajaxPromise
            .done(function (data) { deterred.resolve(data); })
            .fail(function (data) { deterred.reject(data); })
    }, delay);

    return deterred.promise();
};

//override the following code if we are going to change JQuery
directSp.SpApp.prototype._ajaxProvider = function (ajaxOptions) {
    return jQuery.ajax(ajaxOptions);
};

directSp.SpApp.prototype._processApiHook = function (ajaxOptions, hookOptions) {

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

    var deferred = new jQuery.Deferred();

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

        deferred.resolve(data);
    }
    catch (e) {
        deferred.reject(this._convertToError(e)); //make sure hook error converted to SpError
    }

    return deferred.promise();
}

directSp.SpApp.prototype._processPagination = function (spCall, invokeOptions) {

    //prevent recursive call
    if (invokeOptions.pagination == null && invokeOptions.pageSize != null) invokeOptions.pagination = "server";
    if (invokeOptions.pagination == null || invokeOptions.pagination == "" || invokeOptions.pagination == "none")
        return null;

    //create paginator
    var paginator = new directSp.SpApp.Paginator(this, spCall, invokeOptions);
    return paginator;
};

// *********************
// **** Paginator
// *********************
directSp.SpApp.Paginator = function (spApp, spCall, invokeOptions) {

    this._spApp = spApp;
    this._apiCall = spCall;
    this._invokeOptions = invokeOptions;
    this._pagePromises = [];
    this._isInvoked = false;
    this._isCacheUsed = false;
    this.reset();
    this._isCacheInvalidated = false; //clear for first time after reset
    this._pageSize = invokeOptions.pageSize != null ? invokeOptions.pageSize : 20;
    this._pageCacheCount = (invokeOptions.pageCacheCount != null) ? invokeOptions.pageCacheCount : 1;

    if (this._pageSize < 1) throw "pageSize must be greater than 0";
};

directSp.SpApp.Paginator.prototype = {
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

directSp.SpApp.Paginator.prototype.downloadAsTsv = function () {
    var newInvokeOptions = {
        recordsetFileTitle: this._invokeOptions.recordsetFileTitle,
        autoDownload: true
    };

    var promise = this._spApp.invokeApi2(this._apiCall, newInvokeOptions);
    return promise;
},

    directSp.SpApp.Paginator.prototype.getApproxPageCount = function (maxPageCount) {
        var value = Math.max(maxPageCount, this.pageCountMin);
        if (this.pageCountMax != null)
            value = Math.min(value, this.pageCountMax);
        return value;
    },

    directSp.SpApp.Paginator.prototype.goPrevPage = function () {
        return this.goPage(this.pageIndex + 1);
    };

directSp.SpApp.Paginator.prototype.goNextPage = function () {
    return this.goPage(this.pageIndex - 1);
};

directSp.SpApp.Paginator.prototype.refresh = function () {
    var curPageIndex = this.pageIndex;
    this.reset();
    return this.goPage(curPageIndex);
};

directSp.SpApp.Paginator.prototype.reset = function () {
    this._pages = [];
    this._pageCount = null;
    this._pageCountMin = 1;
    this._pageCountMax = null;
    this._pageIndex = 0;
    this._isCacheInvalidated = true;
    this._isCacheUsed = false;
};

directSp.SpApp.Paginator.prototype.goPage = function (pageNo) {
    var _this = this;
    var deferred = new jQuery.Deferred();
    var promise = deferred.promise();
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

        //invokeApi
        this._isInvoked = true;
        this._spApp.invokeApi2(this._apiCall, invokeOptions)
            .done(function (data) {
                var recordset = data.recordset != null ? data.recordset : [];

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

                deferred.resolve(data);
            })
            .fail(function (data) {
                deferred.reject(data);
            });

        //assign promises
        for (var i = pageStart; i <= pageEnd; i++)
            this._pagePromises[i] = promise;
    }

    //change current page after getting result
    return this._pagePromises[pageNo].done(function () {
        _this._pageIndex = pageNo;
    });
};

// *********************
// **** Captcha Controller
// *********************
directSp.SpApp.CaptchaController = function (data) {
    this._data = data;
    this.captchaImageUri = "data:image/png;base64," + data.captchaImage;
};

directSp.SpApp.CaptchaController.prototype.continue = function (captchaCode) {
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

    //try update invokeParams for invokeApi
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
    this._data.spApp._ajaxHelper(ajaxOptions)
        .done(function (data) {
            _this._data.deferred.resolve(data);
        })
        .fail(function (data) {
            _this._data.deferred.reject(data);
        });
}

directSp.SpApp.CaptchaController.prototype.cancel = function () {
    var error = this._data.spApp._convertToError("Captcha has been canceled by the user!");
    this._data.deferred.reject(error);
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
directSp.Html.submit = function (url, fields) {
    var form = jQuery('<form>', {
        action: url,
        method: 'post'
    });
    jQuery.each(fields, function (key, val) {
        jQuery('<input>').attr({
            type: "hidden",
            name: key,
            value: val
        }).appendTo(form);
    });
    $(document.body).append(form);
    form.submit();
};

//Create object
var spApp = new directSp.SpApp();
