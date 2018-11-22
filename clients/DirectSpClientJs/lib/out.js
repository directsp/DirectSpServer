var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    }
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : new P(function (resolve) { resolve(result.value); }).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
define("DirectSpUtil", ["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var Convert = /** @class */ (function () {
        function Convert() {
        }
        Convert.toBoolean = function (value, defaultValue) {
            if (defaultValue === void 0) { defaultValue = false; }
            if (value === null || value === undefined)
                return defaultValue;
            // check is value boolean
            if (typeof value == "boolean")
                return value;
            if (typeof value == "number")
                return value != 0;
            if (typeof value == "string") {
                var parsed = parseInt(value);
                if (!isNaN(parsed))
                    return parsed != 0;
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
            throw new Error("Could not convert " + typeof value + " to boolean!");
        };
        ;
        Convert.toInteger = function (value, defaultValue) {
            if (defaultValue === void 0) { defaultValue = 0; }
            if (value === null || value === undefined)
                return defaultValue;
            return parseInt(value);
        };
        ;
        Convert.toQueryString = function (obj) {
            var parts = [];
            for (var key in obj) {
                if (obj.hasOwnProperty(key)) {
                    parts.push(encodeURIComponent(key) + "=" + encodeURIComponent(obj[key]));
                }
            }
            return parts.join("&");
        };
        ;
        Convert.buffer_fromBase64 = function (base64String) {
            if (window.atob)
                return window.atob(base64String);
            var e = {}, i, b = 0, c, x, l = 0, a, r = "", w = String.fromCharCode, L = base64String.length;
            var A = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
            for (i = 0; i < 64; i++) {
                e[A.charAt(i)] = i;
            }
            for (x = 0; x < L; x++) {
                c = e[base64String.charAt(x)];
                b = (b << 6) + c;
                l += 6;
                while (l >= 8) {
                    ((a = (b >>> (l -= 8)) & 0xff) || x < L - 2) && (r += w(a));
                }
            }
            return r;
        };
        ;
        return Convert;
    }());
    exports.Convert = Convert;
    var Html = /** @class */ (function () {
        function Html() {
        }
        Html.submit = function (url, params) {
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
        return Html;
    }());
    exports.Html = Html;
    var Uri = /** @class */ (function () {
        function Uri() {
        }
        Uri.getUrlWithoutQueryString = function (uri) {
            var i = uri.indexOf("?");
            if (i != -1)
                uri = uri.slice(0, i);
            i = uri.indexOf("#");
            if (i != -1)
                uri = uri.slice(i + 1);
            return uri;
        };
        ;
        Uri.getFileName = function (uri) {
            uri = Uri.getUrlWithoutQueryString(uri);
            var i = uri.indexOf("/");
            if (i != -1)
                uri = uri.slice(i + 1);
            return uri;
        };
        ;
        Uri.getParameterByName = function (name, url) {
            if (!url && window && window.location && window.location.href)
                url = window.location.href;
            if (!url)
                return null;
            name = name.replace(/[\[\]]/g, "\\$&");
            var regex = new RegExp("[#?&]" + name + "(=([^&#]*)|&|#|$)"), results = regex.exec(url);
            if (!results)
                return null;
            if (!results[2])
                return "";
            return decodeURIComponent(results[2].replace(/\+/g, " "));
        };
        Uri.combine = function (uriBase, uriRelative) {
            var parts = [uriBase, uriRelative];
            return parts
                .map(function (path) {
                if (path[0] == "/") {
                    path = path.slice(1);
                }
                if (path[path.length - 1] == "/") {
                    path = path.slice(0, path.length - 1);
                }
                return path;
            })
                .join("/");
        };
        Uri.getParent = function (uri) {
            var endIndex = uri.indexOf("?");
            if (endIndex == -1)
                endIndex = uri.length;
            var lastSlash = uri.lastIndexOf("/", endIndex);
            return uri.substr(0, lastSlash);
        };
        return Uri;
    }());
    exports.Uri = Uri;
    var Utility = /** @class */ (function () {
        function Utility() {
        }
        Utility.checkUndefined = function (value, defValue) {
            return value !== undefined ? value : defValue;
        };
        ;
        Utility.parseJwt = function (token) {
            var base64Url = token.split(".")[1];
            var base64 = base64Url.replace("-", "+").replace("_", "/");
            return JSON.parse(Convert.buffer_fromBase64(base64));
        };
        ;
        Utility.clone = function (obj) {
            if (obj == null)
                return obj;
            return JSON.parse(JSON.stringify(obj));
        };
        ;
        Utility.getRandomInt = function (min, max) {
            min = Math.ceil(min);
            max = Math.floor(max);
            return Math.floor(Math.random() * (max - min)) + min;
        };
        ;
        Utility.generateGuid = function () {
            return Math.random()
                .toString(36)
                .substring(2) + new Date().getTime().toString(36);
        };
        ;
        Utility.toCamelcase = function (str) {
            if (str == null || str == "")
                return str;
            return str.substr(0, 1).toLowerCase() + str.substr(1);
        };
        ;
        //return null if error occur
        Utility.tryParseJason = function (json) {
            try {
                return JSON.parse(json);
            }
            catch (_a) {
                return null;
            }
        };
        ;
        Object.defineProperty(Utility, "isHtmlHost", {
            get: function () {
                return window && window.localStorage != null;
            },
            enumerable: true,
            configurable: true
        });
        return Utility;
    }());
    exports.Utility = Utility;
});
define("DirectSpError", ["require", "exports", "DirectSpUtil"], function (require, exports, DirectSpUtil_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var DirectSpError = /** @class */ (function (_super) {
        __extends(DirectSpError, _super);
        function DirectSpError(message) {
            var _this = _super.call(this, message) || this;
            _this.errorType = null;
            _this.errorName = null;
            _this.errorNumber = null;
            _this.errorMessage = null;
            _this.errorDescription = null;
            _this.errorProcName = null;
            _this.status = null;
            _this.statusText = null;
            _this.innerError = null;
            _this.errorData = null;
            _this.errorMessage = message;
            return _this;
        }
        DirectSpError.create = function (data) {
            // already converted
            if (data instanceof DirectSpError)
                return data;
            // create error
            var error = new DirectSpError("");
            //casting data
            if (data == null) {
                error.errorName = "unknown";
            }
            else if (data == "number") {
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
            else if (data.errorName != null ||
                data.errorNumber != null ||
                data.error != null ||
                data.error_description != null) {
                //copy all data
                error.errorName = DirectSpUtil_1.Utility.checkUndefined(data.errorName, null);
                error.errorNumber = DirectSpUtil_1.Utility.checkUndefined(data.errorNumber, null);
                error.errorMessage = DirectSpUtil_1.Utility.checkUndefined(data.errorMessage, null);
                error.errorType = DirectSpUtil_1.Utility.checkUndefined(data.errorType, null);
                error.errorDescription = DirectSpUtil_1.Utility.checkUndefined(data.errorDescription, null);
                error.errorProcName = DirectSpUtil_1.Utility.checkUndefined(data.errorProcName, null);
                error.status = DirectSpUtil_1.Utility.checkUndefined(data.status, null);
                error.statusText = DirectSpUtil_1.Utility.checkUndefined(data.statusText, null);
                error.innerError = DirectSpUtil_1.Utility.checkUndefined(data.innerError, null);
                //try to convert error
                if (data.error) {
                    error.errorType = "OpenIdConnect";
                    error.errorName = data.error;
                }
                //try to convert error_description
                if (data.error_description) {
                    error.errorDescription = data.error_description;
                }
            }
            else {
                error.errorName = "unknown";
                error.errorDescription = data.toString();
                error.innerError = data;
            }
            //try to reconver actual data from error_description
            if (error.errorDescription) {
                try {
                    var obj = JSON.parse(error.errorDescription);
                    if (obj.errorName || obj.errorNumber)
                        return DirectSpError.create(obj);
                }
                catch (e) { }
            }
            //fix error message
            if (error.errorName)
                error.message = error.errorName;
            if (error.errorMessage)
                error.message = error.errorMessage;
            return error;
        };
        return DirectSpError;
    }(Error));
    exports.DirectSpError = DirectSpError;
    var exceptions;
    (function (exceptions) {
        var NotSupportedException = /** @class */ (function (_super) {
            __extends(NotSupportedException, _super);
            function NotSupportedException(message) {
                var _this = _super.call(this, "The operation is not supported!" + message ? " " + message : "") || this;
                _this.errorType = "Client";
                _this.errorName = "NotSupportedException";
                return _this;
            }
            return NotSupportedException;
        }(DirectSpError));
        exceptions.NotSupportedException = NotSupportedException;
        var NotImplementedException = /** @class */ (function (_super) {
            __extends(NotImplementedException, _super);
            function NotImplementedException(message) {
                var _this = _super.call(this, "The method is not implemeneted!" + message ? " " + message : "") || this;
                _this.errorType = "Client";
                _this.errorName = "NotImplementedException";
                return _this;
            }
            return NotImplementedException;
        }(DirectSpError));
        exceptions.NotImplementedException = NotImplementedException;
    })(exceptions = exports.exceptions || (exports.exceptions = {}));
});
define("DirectSpAjax", ["require", "exports", "DirectSpError", "DirectSpUtil"], function (require, exports, DirectSpError_1, DirectSpUtil_2) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var DirectSpXmlHttpAjaxProvider = /** @class */ (function () {
        function DirectSpXmlHttpAjaxProvider() {
        }
        DirectSpXmlHttpAjaxProvider.prototype.fetch = function (request) {
            return new Promise(function (resolve, reject) {
                var req = new XMLHttpRequest();
                req.withCredentials = DirectSpUtil_2.Convert.toBoolean(request.withCredentials, false);
                req.open(request.method, request.url);
                req.onload = function () {
                    if (req.status == 200) {
                        var response = { data: req.responseText, headers: req.getResponseHeader };
                        resolve(response);
                    }
                    else {
                        var error = null;
                        try {
                            var obj = JSON.parse(req.responseText);
                            error = DirectSpError_1.DirectSpError.create(obj);
                            error.innerError = obj;
                        }
                        catch (err) {
                            var text = req.responseText == "" ? req.statusText : req.responseText;
                            error = DirectSpError_1.DirectSpError.create(text);
                        }
                        error.status = req.status;
                        error.statusText = req.statusText;
                        reject(error);
                    }
                };
                req.onerror = function () {
                    var error = new DirectSpError_1.DirectSpError("Network error or server unreachable!");
                    error.errorName = "Network Error";
                    error.errorNumber = 503;
                    reject(error);
                };
                //headers
                if (request.headers) {
                    for (var item in request.headers) {
                        if (request.headers.hasOwnProperty(item))
                            req.setRequestHeader(item, request.headers[item]);
                    }
                }
                //creating body
                var body = request.data;
                //finding Content-Type
                var contentType = request.headers
                    ? request.headers["Content-Type"]
                    : null;
                if (!contentType)
                    contentType = "application/x-www-form-urlencoded;charset=UTF-8"; //default
                contentType = contentType.toLowerCase();
                //convert data based on contentType
                if (contentType.indexOf("application/json") != -1 && body)
                    body = JSON.stringify(request.data);
                else if (contentType.indexOf("application/x-www-form-urlencoded") != -1)
                    body = DirectSpUtil_2.Convert.toQueryString(request.data);
                //send
                req.send(body);
            });
        };
        return DirectSpXmlHttpAjaxProvider;
    }());
    exports.DirectSpXmlHttpAjaxProvider = DirectSpXmlHttpAjaxProvider;
});
define("DirectSpStorage", ["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var DirectSpHtmlStorage = /** @class */ (function () {
        function DirectSpHtmlStorage(storage) {
            this._storage = storage;
        }
        DirectSpHtmlStorage.prototype.getItem = function (key) {
            var _this = this;
            return new Promise(function (resolve) {
                resolve(_this._storage.getItem(key));
            });
        };
        DirectSpHtmlStorage.prototype.setItem = function (key, value) {
            var _this = this;
            return new Promise(function (resolve) {
                _this._storage.setItem(key, value);
                resolve();
            });
        };
        DirectSpHtmlStorage.prototype.removeItem = function (key) {
            var _this = this;
            return new Promise(function (resolve) {
                _this._storage.removeItem(key);
                resolve();
            });
        };
        return DirectSpHtmlStorage;
    }());
    exports.DirectSpHtmlStorage = DirectSpHtmlStorage;
});
define("DirectSpErrorController", ["require", "exports", "DirectSpError", "DirectSpUtil"], function (require, exports, DirectSpError_2, DirectSpUtil_3) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var DirectSpErrorController = /** @class */ (function () {
        function DirectSpErrorController(data) {
            var _this = this;
            this.captchaCode = null;
            this._captchaImageUri = null;
            this._reject = null;
            this._resolve = null;
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
            this._promise = new Promise(function (resolve, reject) {
                _this._resolve = resolve;
                _this._reject = reject;
                if (!_this._canRetry)
                    reject(data.error);
            });
        }
        ;
        Object.defineProperty(DirectSpErrorController.prototype, "promise", {
            get: function () { return this._promise; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpErrorController.prototype, "error", {
            get: function () { return this._error; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpErrorController.prototype, "captchaImageUri", {
            get: function () { return this._captchaImageUri; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpErrorController.prototype, "canRetry", {
            get: function () { return this._canRetry; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpErrorController.prototype, "dspClient", {
            get: function () { return this._data.dspClient; },
            enumerable: true,
            configurable: true
        });
        DirectSpErrorController.prototype.retry = function () {
            var _this = this;
            var request = this._data.request;
            if (!this.canRetry || !request)
                throw new DirectSpError_2.DirectSpError("Can not retry this error!");
            if (request && this._errorNumber == 55022) {
                var requestData = request.data;
                var invokeOptions = request.data && request.data.invokeOptions ? request.data.invokeOptions : null;
                //try update invokeParams for invoke
                if (invokeOptions && this.error.errorData) {
                    invokeOptions.captchaId = this.error.errorData.captchaId;
                    invokeOptions.captchaCode = this.captchaCode;
                    invokeOptions.requestId = DirectSpUtil_3.Utility.generateGuid();
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
                .then(function (resolve) {
                if (!_this._resolve)
                    throw new DirectSpError_2.DirectSpError("Object is not ready yet!");
                return _this._resolve(resolve);
            })
                .catch(function (error) {
                if (!_this._reject)
                    throw new DirectSpError_2.DirectSpError("Object is not ready yet!");
                return _this._reject(error);
            });
        };
        ;
        DirectSpErrorController.prototype.release = function () {
            if (!this._reject)
                throw new DirectSpError_2.DirectSpError("Object is not ready yet!");
            this._reject(this._error);
        };
        ;
        return DirectSpErrorController;
    }());
    exports.DirectSpErrorController = DirectSpErrorController;
});
define("DirectSpHelp", ["require", "exports", "DirectSpUtil"], function (require, exports, DirectSpUtil_4) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var DirectSpHelp = /** @class */ (function () {
        function DirectSpHelp() {
        }
        DirectSpHelp.help = function (systemApi, criteria) {
            if (criteria === void 0) { criteria = null; }
            var result = "";
            // show help
            if (criteria != null)
                criteria = criteria.trim().toLowerCase();
            if (criteria == "")
                criteria = null;
            //find all proc that match
            var foundProc = [];
            for (var i = 0; i < systemApi.length; i++) {
                if (criteria == null || systemApi[i].procedureName.toLowerCase().indexOf(criteria) != -1) {
                    foundProc.push(systemApi[i]);
                }
            }
            //show procedure if there is only one procedure
            if (foundProc.length == 0) {
                result += ("DirectSp: Nothing found!");
            }
            else {
                for (var i = 0; i < foundProc.length; i++) {
                    if (foundProc[i].procedureName.toLowerCase() == criteria || foundProc.length == 1)
                        result += this._helpImpl(foundProc[i]);
                    else
                        result += (foundProc[i].procedureName) + "\n";
                }
            }
            result += "\n" + "---------------";
            return result;
        };
        ;
        DirectSpHelp._helpImpl = function (procedureMetadata) {
            // find max param length
            var maxParamNameLength = 0;
            var inputParams = [];
            for (var i = 0; i < procedureMetadata.params.length; i++) {
                var param = procedureMetadata.params[i];
                maxParamNameLength = Math.max(maxParamNameLength, param.paramName.length);
                if (!param.isOutput && param.paramName.toLowerCase() != "@context") {
                    inputParams.push(this._formatHelpParamName(param.paramName));
                }
            }
            //prepare input params
            var str = "";
            str += "\n" + "---------------";
            str += "\n" + "Method:";
            str +=
                "\n\t" +
                    procedureMetadata.procedureName +
                    " (" +
                    inputParams.join(", ") +
                    ")";
            str += "\n";
            str += "\n" + "Parameters:";
            for (var i = 0; i < procedureMetadata.params.length; i++) {
                var param = procedureMetadata.params[i];
                if (!param.isOutput && param.paramName.toLowerCase() != "@context") {
                    str +=
                        "\n\t" +
                            this._getHelpParam(procedureMetadata, param, maxParamNameLength);
                }
            }
            //prepare ouput params
            str += "\n";
            str += "\n" + "Returns:";
            for (var i = 0; i < procedureMetadata.params.length; i++) {
                var param = procedureMetadata.params[i];
                if (param.isOutput && param.paramName.toLowerCase() != "@context") {
                    str +=
                        "\n\t" +
                            this._getHelpParam(procedureMetadata, param, maxParamNameLength);
                }
            }
            str +=
                "\n\t" +
                    this._formatHelpParam("returnValue", "integer", maxParamNameLength);
            str +=
                "\n\t" + this._formatHelpParam("recordset", "array", maxParamNameLength);
            //sample
            var sample = 'dspClient.invoke("$(procname)", { $(parameters) })';
            var sampleParam = [];
            for (var i = 0; i < inputParams.length; i++)
                sampleParam.push(inputParams[i] + ": " + "$" + inputParams[i]);
            sample = sample.replace("$(procname)", procedureMetadata.procedureName);
            sample = sample.replace("$(parameters)", sampleParam.join(", "));
            str += "\n";
            str += "\n" + "Sample:";
            str += "\n\t" + sample;
            return str;
        };
        ;
        DirectSpHelp._getHelpParam = function (procedureMetadata, param, maxParamNameLength) {
            var paramType = this._getHelpParamType(param);
            return this._formatHelpParam(param.paramName, paramType, maxParamNameLength);
        };
        ;
        DirectSpHelp._getHelpParamType = function (param) {
            //check userTypeName
            var userTypeName = param.userTypeName != null ? param.userTypeName.toLowerCase() : "";
            if (userTypeName.indexOf("json") != -1)
                return "object";
            //check systemTypeName
            var paramType = param.systemTypeName.toLowerCase();
            if (paramType.indexOf("char") != -1)
                return "string";
            else if (paramType.indexOf("date") != -1)
                return "datetime";
            else if (paramType.indexOf("time") != -1)
                return "datetime";
            else if (paramType.indexOf("money") != -1)
                return "money";
            else if (paramType.indexOf("int") != -1)
                return "integer";
            else if (paramType.indexOf("float") != -1 ||
                paramType.indexOf("decimal") != -1)
                return "float";
            else if (paramType.indexOf("bit") != -1 || paramType.indexOf("decimal") != -1)
                return "boolean";
            return "string";
        };
        ;
        DirectSpHelp._formatHelpParamName = function (paramName) {
            //remove extra characters
            if (paramName.length > 0 && paramName[0] == "@")
                paramName = paramName.substr(1);
            return DirectSpUtil_4.Utility.toCamelcase(paramName);
        };
        ;
        DirectSpHelp._formatHelpParam = function (paramName, paramType, maxParamNameLength) {
            var str = this._formatHelpParamName(paramName);
            //add spaces
            for (var i = str.length; i < maxParamNameLength + 2; i++)
                str += " ";
            return str + "(" + paramType + ")";
        };
        ;
        return DirectSpHelp;
    }());
    exports.DirectSpHelp = DirectSpHelp;
});
define("DirectSpClient", ["require", "exports", "DirectSpAjax", "DirectSpAuth", "DirectSpError", "DirectSpStorage", "DirectSpUtil", "DirectSpErrorController", "DirectSpHelp"], function (require, exports, DirectSpAjax_1, DirectSpAuth_1, DirectSpError_3, DirectSpStorage_1, DirectSpUtil_5, DirectSpErrorController_1, DirectSpHelp_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    /**
     * @see: https://github.com/directsp
     * @event onAuthorized: check the authorization state
     * @event onError: fire when an herror occurred
     * @event: onNewVersion: fire when new API version detected
     * @event: onBeforeInvoke: fire beofore any invoke
     */
    var DirectSpClient = /** @class */ (function () {
        function DirectSpClient(options) {
            this._storageNamePrefix = "DirectSp:";
            this._originalUri = null;
            this._originalQueryString = null;
            this._seqGroups = {};
            this._sessionState = Math.floor(Math.random() * 10000000000000).toString();
            this._resourceAppVersion = null;
            this._systemApi = null;
            //Events
            this.onError = null;
            this.onAuthorized = null;
            this.onNewVersion = null;
            this.onBeforeInvoke = null;
            if (!options.dspLocalStorage) {
                if (!DirectSpUtil_5.Utility.isHtmlHost)
                    throw new DirectSpError_3.DirectSpError("dspLocalStorage has not been set!");
                options.dspLocalStorage = new DirectSpStorage_1.DirectSpHtmlStorage(window.localStorage);
            }
            if (!options.dspSessionStorage) {
                if (!DirectSpUtil_5.Utility.isHtmlHost)
                    throw new DirectSpError_3.DirectSpError("dspSessionStorage has not been set!");
                options.dspSessionStorage = new DirectSpStorage_1.DirectSpHtmlStorage(window.sessionStorage);
            }
            if (!options.resourceApiUri)
                throw new DirectSpError_3.DirectSpError("resourceApiUri has not been set!");
            this._resourceApiUri = options.resourceApiUri;
            this._auth = options.auth && options.auth.clientId && options.auth.clientId != "" ? new DirectSpAuth_1.DirectSpAuth(this, options.auth) : null;
            this._dspLocalStorage = options.dspLocalStorage;
            this._dspSessionStorage = options.dspSessionStorage;
            this._homePageUri = DirectSpUtil_5.Utility.checkUndefined(options.homePageUri, DirectSpUtil_5.Utility.isHtmlHost ? window.location.origin : null);
            this.isAutoReload = DirectSpUtil_5.Utility.checkUndefined(options.isAutoReload, true);
            this.isLogEnabled = DirectSpUtil_5.Utility.checkUndefined(options.isLogEnabled, true);
            this.isUseAppErrorHandler = DirectSpUtil_5.Utility.checkUndefined(options.isUseAppErrorHandler, true);
            this._originalUri = DirectSpUtil_5.Utility.isHtmlHost ? window.location.href : null;
            this._originalQueryString = DirectSpUtil_5.Utility.isHtmlHost ? window.location.search : null;
            this._ajaxProvider = options.ajaxProvider ? options.ajaxProvider : new DirectSpAjax_1.DirectSpXmlHttpAjaxProvider();
        }
        Object.defineProperty(DirectSpClient.prototype, "homePageUri", {
            get: function () { return this._homePageUri; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpClient.prototype, "storageNamePrefix", {
            get: function () { return this._storageNamePrefix; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpClient.prototype, "ajaxProvider", {
            get: function () { return this._ajaxProvider; },
            enumerable: true,
            configurable: true
        });
        ;
        Object.defineProperty(DirectSpClient.prototype, "resourceApiUri", {
            get: function () { return this._resourceApiUri; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpClient.prototype, "auth", {
            get: function () { return this._auth; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpClient.prototype, "originalQueryString", {
            get: function () { return this._originalQueryString; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpClient.prototype, "originalUri", {
            get: function () { return this._originalUri; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpClient.prototype, "resourceAppVersion", {
            get: function () { return this._resourceAppVersion; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpClient.prototype, "dspLocalStorage", {
            get: function () { return this._dspLocalStorage; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpClient.prototype, "dspSessionStorage", {
            get: function () { return this._dspSessionStorage; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpClient.prototype, "sessionState", {
            get: function () { return this._sessionState; },
            enumerable: true,
            configurable: true
        });
        //navigate to directSp auth server
        DirectSpClient.prototype.init = function () {
            var _this = this;
            console.log("DirectSp: initializing ...");
            return this._load().then(function () {
                return _this._auth ? _this._auth.init() : true;
            });
        };
        DirectSpClient.prototype._load = function () {
            var _this = this;
            var promises = [];
            var prefix = this._storageNamePrefix;
            try {
                var promise = null;
                //load resourceAppVersion
                promise = this.dspLocalStorage.getItem(prefix + "resouceAppVersion").then(function (data) {
                    _this._resourceAppVersion = data;
                });
                promises.push(promise);
                //load sessionState; use current session if there is not session
                promise = this.dspSessionStorage.getItem(prefix + "sessionState").then(function (data) {
                    if (data)
                        _this._sessionState = data;
                    else
                        _this.dspSessionStorage.setItem(prefix + "sessionState", _this._sessionState);
                });
                promises.push(promise);
            }
            catch (err) { }
            return Promise.all(promises)
                .then(function () { });
        };
        ;
        /**
         *
         * @param error Will be converted to DirectSpError
         * @param isUseAppErrorHandler
         *  default use global isUseAppErrorHandler
         *  true: use global handler
         *  false just convert and throw the error
         */
        DirectSpClient.prototype.throwAppError = function (error, isUseAppErrorHandler) {
            error = DirectSpError_3.DirectSpError.create(error);
            isUseAppErrorHandler = DirectSpUtil_5.Convert.toBoolean(isUseAppErrorHandler, this.isUseAppErrorHandler);
            if (!this.onError || !isUseAppErrorHandler)
                throw error;
            // create error controller
            var errorController = new DirectSpErrorController_1.DirectSpErrorController({ error: error, dspClient: this });
            this.onError(errorController);
        };
        ;
        DirectSpClient.prototype.invokeBatch = function (spCalls, invokeOptions) {
            var invokeParams = {
                spCalls: spCalls,
                invokeOptions: invokeOptions
            };
            return this._invokeCore(invokeParams);
        };
        ;
        //invokeOptions {seqGroup:"SequenceGroupName ", pageSize:10, pageIndex:0}
        DirectSpClient.prototype.invoke = function (method, params, invokeOptions) {
            var invokeParams = {
                spCall: {
                    method: method,
                    params: params
                },
                invokeOptions: invokeOptions
            };
            return this.invoke2(invokeParams);
        };
        ;
        DirectSpClient.prototype.invoke2 = function (invokeParams) {
            //validate
            if (!invokeParams.spCall)
                throw new DirectSpError_3.DirectSpError("spCall is expected");
            if (!invokeParams.spCall.method)
                throw new DirectSpError_3.DirectSpError("spCall.method is expected");
            if (!this.resourceApiUri)
                throw new DirectSpError_3.DirectSpError("resourceApiUri has not been set!");
            //call api
            return this._invokeCore(invokeParams).then(function (result) {
                // manage auto download
                if (invokeParams.invokeOptions && invokeParams.invokeOptions.autoDownload) {
                    if (!DirectSpUtil_5.Utility.isHtmlHost)
                        throw new DirectSpError_3.exceptions.NotSupportedException("autoDownload");
                    window.location = result.recordsetUri;
                }
                return result;
            });
        };
        ;
        // Invoke Preperation
        // manage seqGroup, write log and append requestId
        DirectSpClient.prototype._invokeCore = function (invokeParams) {
            var _this = this;
            //set default options
            if (!invokeParams.invokeOptions)
                invokeParams.invokeOptions = {};
            if (invokeParams.invokeOptions.autoDownload == true) {
                invokeParams.invokeOptions.isWithRecodsetDownloadUri = true;
                if (!invokeParams.invokeOptions.recordCount)
                    invokeParams.invokeOptions.recordCount = -2;
                if (!invokeParams.invokeOptions.recordsetFormat)
                    invokeParams.invokeOptions.recordsetFormat = "tabSeparatedValues";
            }
            //set defaults
            if (!invokeParams.invokeOptions)
                invokeParams.invokeOptions = {};
            invokeParams.invokeOptions.cache == DirectSpUtil_5.Convert.toBoolean(invokeParams.invokeOptions.cache, true);
            invokeParams.invokeOptions.isUseAppErrorHandler = DirectSpUtil_5.Convert.toBoolean(invokeParams.invokeOptions.isUseAppErrorHandler, this.isUseAppErrorHandler);
            //log request
            var method = invokeParams.spCall ? invokeParams.spCall.method : "invokeBatch";
            if (this.isLogEnabled)
                console.log("DirectSp: invoke (Request)", method, invokeParams);
            // check seqGroup
            var seqGroup = invokeParams.invokeOptions.seqGroup ? invokeParams.invokeOptions.seqGroup : null;
            var seqGroupValue;
            if (seqGroup) {
                seqGroupValue = this._seqGroups[seqGroup]
                    ? this._seqGroups[seqGroup] + 1
                    : 1;
                this._seqGroups[seqGroup] = seqGroupValue;
            }
            // append request id to invoke options
            invokeParams.invokeOptions.requestId = DirectSpUtil_5.Utility.generateGuid();
            //invoke
            return this._invokeCore2(invokeParams)
                .then(function (result) {
                if (seqGroup && seqGroupValue && seqGroupValue != _this._seqGroups[seqGroup])
                    throw new DirectSpError_3.DirectSpError("request has been suppressed by seqGroup!");
                //log response
                if (_this.isLogEnabled)
                    console.log("DirectSp: invoke (Response)", method, invokeParams, result);
                return result;
            })
                .catch(function (error) {
                if (seqGroup && seqGroupValue && seqGroupValue != _this._seqGroups[seqGroup])
                    throw new DirectSpError_3.DirectSpError("request has been suppressed by seqGroup!");
                if (_this.isLogEnabled)
                    console.warn("DirectSp: invoke (Response)", method, invokeParams, error);
                throw error;
            });
        };
        ;
        //Handle Hook and delay
        DirectSpClient.prototype._invokeCore2 = function (invokeParams) {
            return __awaiter(this, void 0, void 0, function () {
                var hookParams, result;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            hookParams = {
                                invokeParams: invokeParams,
                                delay: 0,
                                isHandled: false
                            };
                            return [4 /*yield*/, this._processInvokeHook(hookParams)];
                        case 1:
                            result = _a.sent();
                            if (!hookParams.isHandled)
                                result = this._invokeCore3(hookParams.invokeParams);
                            //return the promise if there is no api delay
                            if (hookParams.delay == null || hookParams.delay <= 0)
                                return [2 /*return*/, result];
                            //proces delay
                            return [2 /*return*/, new Promise(function (resolve) {
                                    var interval = hookParams.delay;
                                    var delay = DirectSpUtil_5.Utility.getRandomInt(interval / 2, interval + interval / 2);
                                    console.warn("DirectSp: Warning! {method} is delayed by {delay} milliseconds");
                                    setTimeout(function () { return resolve(); }, delay);
                                })];
                    }
                });
            });
        };
        ;
        // Convert to fetch
        DirectSpClient.prototype._invokeCore3 = function (invokeParams) {
            return __awaiter(this, void 0, void 0, function () {
                var method, result;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            method = invokeParams.spCall ? invokeParams.spCall.method : "invokeBatch";
                            return [4 /*yield*/, this._fetch({
                                    url: DirectSpUtil_5.Uri.combine(this.resourceApiUri, method),
                                    data: invokeParams,
                                    method: "POST",
                                    headers: {
                                        authorization: this.auth ? this.auth.authorizationHeader : null,
                                        "Content-Type": "application/json;charset=UTF-8"
                                    },
                                    cache: invokeParams.invokeOptions ? invokeParams.invokeOptions.cache : false
                                })];
                        case 1:
                            result = _a.sent();
                            this._checkNewVersion(result.headers["DSP-AppVersion"]);
                            return [2 /*return*/, JSON.parse(result.data)];
                    }
                });
            });
        };
        ;
        //manage onError
        DirectSpClient.prototype._fetch = function (request) {
            return __awaiter(this, void 0, void 0, function () {
                var errorController, invokeOptions, isUseAppHandler;
                return __generator(this, function (_a) {
                    try {
                        return [2 /*return*/, this._fetch2(request)];
                    }
                    catch (error) {
                        errorController = new DirectSpErrorController_1.DirectSpErrorController({ error: error, dspClient: this, request: request });
                        invokeOptions = request.data && request.data.invokeOptions ? request.data.invokeOptions : null;
                        isUseAppHandler = invokeOptions && invokeOptions.isUseAppErrorHandler;
                        //we should call onError if the exception can be retried
                        if (this.onError && (errorController.canRetry || isUseAppHandler)) {
                            if (this.isLogEnabled)
                                console.log("DirectSp: Calling onError ...");
                            this.onError(errorController);
                            return [2 /*return*/, errorController.promise];
                        }
                        throw error;
                    }
                    return [2 /*return*/];
                });
            });
        };
        //manage RefreshToken
        DirectSpClient.prototype._fetch2 = function (request) {
            return __awaiter(this, void 0, void 0, function () {
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            //check if request require authentication
                            if (!this.auth || !request.headers || !request.headers.authorization)
                                return [2 /*return*/, this._fetchProvider(request)]; //there is no token
                            //refreshing token
                            if (request.data && request.data.grant_type == 'refresh_token')
                                return [2 /*return*/, this._fetchProvider(request)]; //prevent loop
                            return [4 /*yield*/, this.auth.refreshToken()];
                        case 1:
                            _a.sent();
                            request.headers.authorization = this.auth.authorizationHeader; //update request token
                            // fetch again with valid token
                            return [2 /*return*/, this._fetch(request)];
                    }
                });
            });
        };
        DirectSpClient.prototype._fetchProvider = function (request) {
            return this._ajaxProvider.fetch(request);
        };
        ;
        /**
         * @param resourceAppVersion version which is retrieved from the server
         * @returns true current version is different from server version
         */
        DirectSpClient.prototype._checkNewVersion = function (resourceAppVersion) {
            // app versin does not available if resourceAppVersion is null
            if (!resourceAppVersion || resourceAppVersion == this.resourceAppVersion)
                return false;
            //detect new versio
            var isReloadNeeded = this.resourceAppVersion != null &&
                this.resourceAppVersion != resourceAppVersion;
            // save new version
            this._resourceAppVersion = resourceAppVersion;
            this.dspLocalStorage.setItem(this._storageNamePrefix + "resouceAppVersion", resourceAppVersion);
            // reloading
            if (isReloadNeeded) {
                //call new version event
                if (this.onNewVersion)
                    this.onNewVersion();
                // auto reload page
                if (this.isAutoReload && DirectSpUtil_5.Utility.isHtmlHost) {
                    console.log("DirectSp: New version detected! Reloading ...");
                    window.location.reload(true);
                }
            }
            return isReloadNeeded;
        };
        ;
        DirectSpClient.prototype._processInvokeHook = function (hookParams) {
            return __awaiter(this, void 0, void 0, function () {
                var result, e_1;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            //return quickly if there is no hook
                            if (!this.onBeforeInvoke)
                                return [2 /*return*/, {}];
                            // batch does not supported
                            if (!hookParams.invokeParams.spCall || hookParams.invokeParams.spCall.method == 'invokeBatch')
                                return [2 /*return*/, {}];
                            _a.label = 1;
                        case 1:
                            _a.trys.push([1, 3, , 4]);
                            return [4 /*yield*/, this.onBeforeInvoke(hookParams)];
                        case 2:
                            result = _a.sent();
                            if (hookParams.isHandled)
                                //log hook
                                if (this.isLogEnabled)
                                    console.warn("DirectSp: Hooking > ", hookParams.invokeParams.spCall.method, hookParams, result);
                            return [2 /*return*/, DirectSpUtil_5.Utility.clone(result)];
                        case 3:
                            e_1 = _a.sent();
                            // convert user errors to DirectSpError
                            throw DirectSpError_3.DirectSpError.create(e_1);
                        case 4: return [2 /*return*/];
                    }
                });
            });
        };
        ;
        DirectSpClient.prototype.help = function (criteria, reload) {
            if (criteria === void 0) { criteria = null; }
            if (reload === void 0) { reload = false; }
            return __awaiter(this, void 0, void 0, function () {
                var result;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            if (!(!this._systemApi || reload)) return [3 /*break*/, 2];
                            return [4 /*yield*/, this.invoke("System_Api")];
                        case 1:
                            result = _a.sent();
                            this._systemApi = result.api;
                            if (!result.api)
                                return [2 /*return*/, "DirectSp: Could not retreive api information!"];
                            _a.label = 2;
                        case 2: return [2 /*return*/, DirectSpHelp_1.DirectSpHelp.help(this._systemApi, criteria)];
                    }
                });
            });
        };
        ;
        return DirectSpClient;
    }());
    exports.DirectSpClient = DirectSpClient;
    ;
});
define("DirectSpAuth", ["require", "exports", "DirectSpError", "DirectSpUtil"], function (require, exports, DirectSpError_4, DirectSpUtil_6) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var DirectSpAuth = /** @class */ (function () {
        function DirectSpAuth(dspClient, options) {
            this.isAutoSignIn = false;
            this._redirectUri = null;
            this._baseEndpointUri = null;
            this._authorizeEndpointUri = null;
            this._tokenEndpointUri = null;
            this._userinfoEndpointUri = null;
            this._logoutEndpointUri = null;
            this._scope = "offline_access";
            this._type = "token";
            this._refreshClockSkew = 60;
            this._authRequest = null;
            this._userInfo = null;
            this._isPersistent = false;
            this._tokens = null;
            this._authError = null;
            this._accessTokenInfo = null;
            this._lastPageUri = null;
            this._dspClient = dspClient;
            this._lastPageUri = dspClient.homePageUri;
            this._clientId = DirectSpUtil_6.Utility.checkUndefined(options.clientId, "");
            this._redirectUri = DirectSpUtil_6.Utility.checkUndefined(options.redirectUri, DirectSpUtil_6.Utility.isHtmlHost ? window.location.origin + "/oauth2/callback" : null);
            this._scope = DirectSpUtil_6.Utility.checkUndefined(options.scope, this._scope); //openid offline_access profile phone email address
            this._type = DirectSpUtil_6.Utility.checkUndefined(options.type, this._type); //token, code
            this._isPersistent = DirectSpUtil_6.Utility.checkUndefined(options.isPersistent, this._isPersistent);
            this.isAutoSignIn = DirectSpUtil_6.Utility.checkUndefined(options.isAutoSignIn, this.isAutoSignIn);
            // must set before other endpoint to prevent override
            if (options.baseEndpointUri) {
                this._baseEndpointUri = options.baseEndpointUri;
                this._authorizeEndpointUri = DirectSpUtil_6.Uri.combine(options.baseEndpointUri, "/connect/authorize");
                this._tokenEndpointUri = DirectSpUtil_6.Uri.combine(options.baseEndpointUri, "/connect/token");
                this._userinfoEndpointUri = DirectSpUtil_6.Uri.combine(options.baseEndpointUri, "/connect/userinfo");
                this._logoutEndpointUri = DirectSpUtil_6.Uri.combine(options.baseEndpointUri, "/connect/logout");
            }
            if (options.authorizeEndpointUri)
                this._authorizeEndpointUri = options.authorizeEndpointUri;
            if (options.tokenEndpointUri)
                this._tokenEndpointUri = options.tokenEndpointUri;
            if (options.userinfoEndpointUri)
                this._userinfoEndpointUri = options.userinfoEndpointUri;
            if (options.logoutEndpointUri)
                this._logoutEndpointUri = options.logoutEndpointUri;
            var requestClientId = DirectSpUtil_6.Uri.getParameterByName("client_id");
            if (requestClientId) {
                this._authRequest =
                    {
                        client_id: requestClientId,
                        redirect_uri: DirectSpUtil_6.Uri.getParameterByName("redirect_uri"),
                        scope: DirectSpUtil_6.Uri.getParameterByName("scope"),
                        response_type: DirectSpUtil_6.Uri.getParameterByName("response_type"),
                        state: DirectSpUtil_6.Uri.getParameterByName("state")
                    };
            }
            this._accessTokenInfo = null;
        }
        Object.defineProperty(DirectSpAuth.prototype, "clientId", {
            get: function () { return this._clientId; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "redirectUri", {
            get: function () { return this._redirectUri; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "authorizeEndpointUri", {
            get: function () { return this._authorizeEndpointUri; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "tokenEndpointUri", {
            get: function () { return this._tokenEndpointUri; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "userinfoEndpointUri", {
            get: function () { return this._userinfoEndpointUri; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "logoutEndpointUri", {
            get: function () { return this._logoutEndpointUri; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "type", {
            get: function () { return this._type; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "scope", {
            get: function () { return this._scope; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "tokens", {
            get: function () { return this._tokens; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "authError", {
            get: function () { return this._authError; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "accessTokenInfo", {
            get: function () { return this._accessTokenInfo; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "isAuthorized", {
            get: function () { return this._tokens != null; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "userId", {
            get: function () { return this.accessTokenInfo ? this.accessTokenInfo.sub : null; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "userInfo", {
            get: function () { return this._userInfo; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "dspClient", {
            get: function () { return this._dspClient; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "storageNamePrefix", {
            get: function () { return this._dspClient.storageNamePrefix; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "isPersistent", {
            get: function () { return this._isPersistent; },
            set: function (value) {
                this._isPersistent = value;
                this.dspClient.dspLocalStorage.setItem(this.storageNamePrefix + "isPersistent", value.toString());
            },
            enumerable: true,
            configurable: true
        });
        DirectSpAuth.prototype._setUserInfo = function (value) {
            this._userInfo = value;
            if (value)
                this.dspClient.dspLocalStorage.setItem(this.storageNamePrefix + "userInfo", JSON.stringify(value));
            else
                this.dspClient.dspLocalStorage.removeItem(this.storageNamePrefix + "userInfo");
        };
        Object.defineProperty(DirectSpAuth.prototype, "authorizationHeader", {
            get: function () {
                return this.tokens
                    ? this.tokens.token_type + " " + this.tokens.access_token
                    : null;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "username", {
            get: function () {
                return this.userInfo ? this.userInfo.username : null;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "userDisplayName", {
            get: function () {
                if (!this.userInfo)
                    return null;
                return this.userInfo.name ? this.userInfo.name : this.userInfo.username;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "baseEndpointUri", {
            get: function () {
                return this._baseEndpointUri;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "authRequest", {
            get: function () { return this._authRequest; },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "authRequestUri", {
            get: function () {
                if (!DirectSpUtil_6.Utility.isHtmlHost)
                    throw new DirectSpError_4.exceptions.NotSupportedException();
                return (window.location.origin + "?" + DirectSpUtil_6.Convert.toQueryString(this._authRequest));
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "isAuthRequest", {
            get: function () {
                return this.authRequest && this.authRequest.client_id && this.authRequest.state;
            },
            enumerable: true,
            configurable: true
        });
        Object.defineProperty(DirectSpAuth.prototype, "isAuthCallback", {
            get: function () {
                var callbackPattern = this.authorizeEndpointUri;
                return callbackPattern && location && location.href && location.href.indexOf(callbackPattern) != -1;
            },
            enumerable: true,
            configurable: true
        });
        DirectSpAuth.prototype.init = function () {
            return __awaiter(this, void 0, void 0, function () {
                var result, error_1;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            //process error
                            if (DirectSpUtil_6.Uri.getParameterByName("error") != null) {
                                this._authError = DirectSpError_4.DirectSpError.create({
                                    error: DirectSpUtil_6.Uri.getParameterByName("error"),
                                    error_description: DirectSpUtil_6.Uri.getParameterByName("error_description")
                                });
                                console.error("DirectSp: Auth Error!", this._authError);
                                if (this.isAutoSignIn) {
                                    this.isAutoSignIn = false;
                                    console.warn("DirectSp: isAutoSignIn is set to false due Auth Error", this.authError);
                                }
                            }
                            _a.label = 1;
                        case 1:
                            _a.trys.push([1, 5, , 6]);
                            return [4 /*yield*/, this._load()];
                        case 2:
                            _a.sent();
                            return [4 /*yield*/, this.refreshToken()];
                        case 3:
                            _a.sent();
                            return [4 /*yield*/, this._processAuthCallback()];
                        case 4:
                            result = _a.sent();
                            this._fireAuthorizedEvent();
                            return [2 /*return*/, result];
                        case 5:
                            error_1 = _a.sent();
                            this._fireAuthorizedEvent();
                            throw error_1;
                        case 6: return [2 /*return*/];
                    }
                });
            });
        };
        DirectSpAuth.prototype._load = function () {
            return __awaiter(this, void 0, void 0, function () {
                var promises, prefix, localStorage, sessionStorage, promise;
                var _this = this;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            promises = [];
                            prefix = this.storageNamePrefix;
                            localStorage = this.dspClient.dspLocalStorage;
                            sessionStorage = this.dspClient.dspLocalStorage;
                            try {
                                promise = void 0;
                                //tokens
                                promise = sessionStorage.getItem(prefix + "auth.tokens").then(function (data) {
                                    if (!data)
                                        return localStorage.getItem(prefix + "auth.tokens");
                                    return data;
                                }).then(function (data) {
                                    if (data)
                                        _this._tokens = JSON.parse(data);
                                });
                                promises.push(promise);
                                //userInfo
                                promise = sessionStorage.getItem(prefix + "userInfo").then(function (data) {
                                    if (!data)
                                        return localStorage.getItem(prefix + "userInfo");
                                    return data;
                                }).then(function (data) {
                                    if (data)
                                        _this._userInfo = JSON.parse(data);
                                });
                                //isPersistent
                                promise = localStorage.getItem(prefix + "isPersistent").then(function (data) {
                                    if (data != null)
                                        _this._isPersistent = DirectSpUtil_6.Convert.toBoolean(data);
                                });
                                promises.push(promise);
                                promise = this.dspClient.dspSessionStorage.getItem(prefix + "lastPageUri").then(function (data) {
                                    if (data)
                                        _this._lastPageUri = data;
                                });
                                promises.push(promise);
                            }
                            catch (err) { }
                            return [4 /*yield*/, Promise.all(promises)];
                        case 1:
                            _a.sent();
                            return [2 /*return*/];
                    }
                });
            });
        };
        ;
        //navigate to directSp authorization server
        DirectSpAuth.prototype._processAuthCallback = function () {
            return __awaiter(this, void 0, void 0, function () {
                var state, code, data, response, tokens, error_2, access_token, token_type;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            //check is oauth2callback
                            if (!this.isAuthCallback)
                                return [2 /*return*/, Promise.resolve(false)];
                            state = DirectSpUtil_6.Uri.getParameterByName("state");
                            if (this.dspClient.sessionState != state) {
                                this.setTokens(null);
                                throw new DirectSpError_4.DirectSpError("Invalid sessionState!");
                            }
                            code = DirectSpUtil_6.Uri.getParameterByName("code");
                            if (!(code != null)) return [3 /*break*/, 4];
                            //validating configuration
                            if (!this.authorizeEndpointUri)
                                throw new DirectSpError_4.DirectSpError("authorizeEndpointUri has not been set!");
                            if (!this.tokenEndpointUri)
                                throw new DirectSpError_4.DirectSpError("tokenEndpointUri has not been set!");
                            if (!this.redirectUri)
                                throw new DirectSpError_4.DirectSpError("redirectUri has not been set!");
                            data = {
                                client_id: this.clientId,
                                redirect_uri: this.redirectUri,
                                grant_type: "authorization_code",
                                code: code
                            };
                            _a.label = 1;
                        case 1:
                            _a.trys.push([1, 3, , 4]);
                            return [4 /*yield*/, this.dspClient._fetch({
                                    url: this.tokenEndpointUri,
                                    headers: { "Content-Type": "application/x-www-form-urlencoded;charset=UTF-8" },
                                    data: data,
                                    method: "POST"
                                })];
                        case 2:
                            response = _a.sent();
                            tokens = JSON.parse(response.data);
                            return [2 /*return*/, this.setTokens(tokens)];
                        case 3:
                            error_2 = _a.sent();
                            this.setTokens(null);
                            throw error_2;
                        case 4:
                            access_token = DirectSpUtil_6.Uri.getParameterByName("access_token");
                            if (!access_token) return [3 /*break*/, 6];
                            token_type = DirectSpUtil_6.Uri.getParameterByName("token_type");
                            if (!token_type)
                                throw new DirectSpError_4.DirectSpError("access_Token returned without token_type!");
                            // set new token
                            return [4 /*yield*/, this.setTokens({
                                    access_token: access_token,
                                    token_type: token_type,
                                    expires_in: DirectSpUtil_6.Convert.toInteger(DirectSpUtil_6.Uri.getParameterByName("expires_in"), 0),
                                    refresh_token: null
                                })];
                        case 5:
                            // set new token
                            _a.sent();
                            _a.label = 6;
                        case 6: 
                        //finish processAuthCallback without any result
                        return [2 /*return*/, false];
                    }
                });
            });
        };
        ;
        DirectSpAuth.prototype.setTokens = function (value, refreshUserInfo) {
            if (refreshUserInfo === void 0) { refreshUserInfo = true; }
            return __awaiter(this, void 0, void 0, function () {
                var prefix, tokenString;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            // check is token changed
                            if (value == this._tokens)
                                return [2 /*return*/, Promise.resolve(true)]; //no change
                            //set token
                            this._tokens = value;
                            if (!(value != null)) return [3 /*break*/, 2];
                            //clear token if it was invalid
                            try {
                                this._accessTokenInfo = DirectSpUtil_6.Utility.parseJwt(value.access_token);
                            }
                            catch (error) {
                                return [2 /*return*/, this.setTokens(null)];
                            }
                            if (!refreshUserInfo) return [3 /*break*/, 2];
                            return [4 /*yield*/, this._refreshUserInfo()];
                        case 1:
                            _a.sent();
                            _a.label = 2;
                        case 2:
                            prefix = this.dspClient.storageNamePrefix;
                            tokenString = JSON.stringify(this._tokens);
                            //save tokens
                            this.dspClient.dspSessionStorage.setItem(prefix + "auth.tokens", tokenString);
                            if (this.isPersistent)
                                this.dspClient.dspLocalStorage.setItem(prefix + "auth.tokens", tokenString);
                            else
                                this.dspClient.dspLocalStorage.removeItem(prefix + "auth.tokens");
                            // return the token
                            return [2 /*return*/, value != null];
                    }
                });
            });
        };
        ;
        DirectSpAuth.prototype._refreshUserInfo = function () {
            return __awaiter(this, void 0, void 0, function () {
                var _a, e_2;
                return __generator(this, function (_b) {
                    switch (_b.label) {
                        case 0:
                            _b.trys.push([0, 2, , 3]);
                            _a = this;
                            return [4 /*yield*/, this._getUserInfo()];
                        case 1:
                            _a._userInfo = _b.sent();
                            this._setUserInfo(this._userInfo);
                            return [2 /*return*/, true];
                        case 2:
                            e_2 = _b.sent();
                            return [2 /*return*/, false];
                        case 3: return [2 /*return*/];
                    }
                });
            });
        };
        ;
        DirectSpAuth.prototype._getUserInfo = function () {
            return __awaiter(this, void 0, void 0, function () {
                var error, error, request, result, userInfo;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            if (!this.isAuthorized) {
                                error = new DirectSpError_4.DirectSpError("Can not refresh token for unauthorized users");
                                error.errorName = "unauthorized";
                                throw error;
                            }
                            if (!this.userinfoEndpointUri) {
                                error = new DirectSpError_4.DirectSpError("userinfoEndpointUri has not been set!");
                                error.errorName = "bad_configuration";
                                throw error;
                            }
                            request = {
                                url: this.userinfoEndpointUri,
                                method: "GET",
                                headers: { authorization: this.authorizationHeader }
                            };
                            // refresh token
                            return [4 /*yield*/, this.refreshToken()];
                        case 1:
                            // refresh token
                            _a.sent();
                            return [4 /*yield*/, this.dspClient._fetch(request)];
                        case 2:
                            result = _a.sent();
                            userInfo = JSON.parse(result);
                            if (this.dspClient.isLogEnabled)
                                console.log("DirectSp: userInfo", userInfo);
                            return [2 /*return*/, userInfo];
                    }
                });
            });
        };
        ;
        DirectSpAuth.prototype._isTokenExpiredError = function (error) {
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
        ;
        // return false if the current token is not valid
        DirectSpAuth.prototype.refreshToken = function () {
            return __awaiter(this, void 0, void 0, function () {
                var dateNow, request, result, error_3;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            //check expiration time
                            if (this.accessTokenInfo && this.accessTokenInfo["exp"]) {
                                dateNow = new Date();
                                if (parseInt(this.accessTokenInfo["exp"]) - this._refreshClockSkew > dateNow.getTime() / 1000)
                                    return [2 /*return*/]; //token is not refreshed because it is not required
                            }
                            //return false if token not exists
                            if (!this.tokens || !this.tokens.refresh_token || !this.accessTokenInfo) {
                                //current token is not valid and can not be refreshed
                                if (this.isAuthorized)
                                    this.signOut(false, this.isAutoSignIn);
                                return [2 /*return*/];
                            }
                            //Refreshing token
                            console.log("DirectSp: Refreshing current token ...");
                            if (!this.tokenEndpointUri)
                                throw new DirectSpError_4.DirectSpError("tokenEndpointUri has not been set for refreshing token!");
                            request = {
                                url: this.tokenEndpointUri,
                                method: "POST",
                                headers: {
                                    "Content-Type": "application/x-www-form-urlencoded;charset=UTF-8",
                                    authorization: this.authorizationHeader
                                },
                                data: {
                                    grant_type: "refresh_token",
                                    client_id: this.clientId,
                                    refresh_token: this.tokens.refresh_token,
                                }
                            };
                            _a.label = 1;
                        case 1:
                            _a.trys.push([1, 3, , 4]);
                            return [4 /*yield*/, this.dspClient._fetch(request)];
                        case 2:
                            result = _a.sent();
                            this.setTokens(JSON.parse(result.data), false);
                            return [3 /*break*/, 4];
                        case 3:
                            error_3 = _a.sent();
                            if (this.isAuthorized && this._isTokenExpiredError(error_3))
                                this.signOut(false, this.isAutoSignIn);
                            throw error_3;
                        case 4: return [2 /*return*/];
                    }
                });
            });
        };
        ;
        DirectSpAuth.prototype._resetUser = function () {
            this.setTokens(null);
            this._setUserInfo(null);
        };
        ;
        DirectSpAuth.prototype._fireAuthorizedEvent = function () {
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
                if (_this.dspClient.onAuthorized) {
                    var data = { lastPageUri: _this._lastPageUri };
                    _this.dspClient.onAuthorized(data);
                }
                _this._lastPageUri = null; //reset last page notifying user
                _this.dspClient.dspSessionStorage.removeItem(_this.storageNamePrefix + "lastPageUri");
            }, 0);
        };
        ;
        DirectSpAuth.prototype.signInByPasswordGrant = function (username, password) {
            return __awaiter(this, void 0, void 0, function () {
                var orgIsAuthorized, requestParam, result, token, error_4;
                return __generator(this, function (_a) {
                    switch (_a.label) {
                        case 0:
                            if (!this.tokenEndpointUri)
                                throw new DirectSpError_4.DirectSpError("tokenEndpointUri has not been set!");
                            orgIsAuthorized = this.isAuthorized;
                            //clear user info and tokens
                            if (this.username != username) {
                                this._resetUser();
                            }
                            requestParam = {
                                username: username,
                                password: password,
                                grant_type: "password",
                                scope: this.scope,
                                client_id: this.clientId
                            };
                            _a.label = 1;
                        case 1:
                            _a.trys.push([1, 3, , 4]);
                            return [4 /*yield*/, this.dspClient._fetch({
                                    url: this.tokenEndpointUri,
                                    method: "POST",
                                    headers: {
                                        "Content-Type": "application/x-www-form-urlencoded;charset=UTF-8"
                                    },
                                    data: requestParam,
                                })];
                        case 2:
                            result = _a.sent();
                            token = JSON.parse(result.data);
                            this._fireAuthorizedEvent();
                            return [2 /*return*/, token];
                        case 3:
                            error_4 = _a.sent();
                            if (orgIsAuthorized)
                                this._fireAuthorizedEvent();
                            throw error_4;
                        case 4: return [2 /*return*/];
                    }
                });
            });
        };
        //signOut but keep current username
        DirectSpAuth.prototype.signOut = function (clearUser, redirect) {
            if (clearUser === void 0) { clearUser = true; }
            if (redirect === void 0) { redirect = false; }
            // clear access_tokens
            this.setTokens(null);
            // clear all user info
            if (clearUser)
                this._resetUser();
            //redirect to signout page
            if (redirect && this.redirectUri) {
                var params = {
                    client_id: this.clientId,
                    redirect_uri: this.redirectUri,
                    scope: this.scope,
                    response_type: this.type,
                    state: this.dspClient.sessionState
                };
                this.isAutoSignIn = false; //let leave the page
                window.location.href =
                    this.logoutEndpointUri + "?" + DirectSpUtil_6.Convert.toQueryString(params);
            }
            // always fire AuthorizedEvent
            this._fireAuthorizedEvent();
        };
        ;
        DirectSpAuth.prototype.grantAuthorization = function (password) {
            if (!this.authorizeEndpointUri)
                throw new DirectSpError_4.DirectSpError("authorizeEndpointUr has not been set!");
            var requestParam = this.authRequest;
            requestParam.SpApp_Authorization = this.authorizationHeader;
            requestParam.permission = "grant";
            if (password != null)
                requestParam.password = password;
            //submit
            var url = this.authorizeEndpointUri;
            if (this.dspClient.originalQueryString)
                url = url + this.dspClient.originalQueryString;
            DirectSpUtil_6.Html.submit(url, requestParam);
        };
        ;
        DirectSpAuth.prototype.denyAuthorization = function () {
            if (!this.authorizeEndpointUri)
                throw new DirectSpError_4.DirectSpError("authorizeEndpointUr has not been set!");
            var requestParam = this.authRequest;
            requestParam.SpApp_Authorization = this.authorizationHeader;
            requestParam.permission = "deny";
            //submit
            var url = this.authorizeEndpointUri;
            if (this.dspClient.originalQueryString)
                url = url + this.dspClient.originalQueryString;
            DirectSpUtil_6.Html.submit(url, requestParam);
        };
        ;
        //navigate to directSp auth server
        DirectSpAuth.prototype.signIn = function () {
            if (!DirectSpUtil_6.Utility.isHtmlHost)
                throw new DirectSpError_4.exceptions.NotSupportedException();
            //save current location
            this.dspClient.dspSessionStorage.setItem(this.storageNamePrefix + "lastPageUri", window.location.href);
            //redirect to sign in
            var params = {
                client_id: this.clientId,
                redirect_uri: this.redirectUri,
                scope: this.scope,
                response_type: this.type,
                state: this.dspClient.sessionState
            };
            window.location.href = this.authorizeEndpointUri + "?" + DirectSpUtil_6.Convert.toQueryString(params);
        };
        ;
        return DirectSpAuth;
    }());
    exports.DirectSpAuth = DirectSpAuth;
});
define("directSp", ["require", "exports", "DirectSpAjax", "DirectSpStorage", "DirectSpError", "DirectSpClient", "DirectSpUtil", "DirectSpStorage", "DirectSpError", "DirectSpClient", "DirectSpError", "DirectSpUtil"], function (require, exports, DirectSpAjax_2, DirectSpStorage_2, DirectSpError_5, DirectSpClient_1, DirectSpUtil_7, DirectSpStorage_3, DirectSpError_6, DirectSpClient_2, DirectSpError_7, DirectSpUtil_8) {
    "use strict";
    function __export(m) {
        for (var p in m) if (!exports.hasOwnProperty(p)) exports[p] = m[p];
    }
    Object.defineProperty(exports, "__esModule", { value: true });
    __export(DirectSpAjax_2);
    __export(DirectSpStorage_2);
    __export(DirectSpError_5);
    __export(DirectSpClient_1);
    __export(DirectSpUtil_7);
    var directSp;
    (function (directSp) {
        directSp.DirectSpHtmlStorage = DirectSpStorage_3.DirectSpHtmlStorage;
        directSp.DirectSpError = DirectSpError_6.DirectSpError;
        directSp.DirectSpClient = DirectSpClient_2.DirectSpClient;
        directSp.Html = DirectSpUtil_8.Html;
        directSp.Uri = DirectSpUtil_8.Uri;
        directSp.Convert = DirectSpUtil_8.Convert;
        directSp.Utility = DirectSpUtil_8.Utility;
        directSp.exceptions = DirectSpError_7.exceptions;
    })(directSp || (directSp = {}));
    window.directSp = directSp;
});
