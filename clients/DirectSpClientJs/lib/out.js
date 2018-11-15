var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
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
define("util/Convert", ["require", "exports"], function (require, exports) {
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
    exports.default = Convert;
});
define("util/Utility", ["require", "exports", "util/Convert"], function (require, exports, Convert_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    Convert_1 = __importDefault(Convert_1);
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
            return JSON.parse(Convert_1.default.buffer_fromBase64(base64));
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
        Utility.isHtmlHost = function () {
            return window && window.localStorage != null;
        };
        return Utility;
    }());
    exports.default = Utility;
});
define("DirectSpError", ["require", "exports", "util/Utility"], function (require, exports, Utility_1) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    Utility_1 = __importDefault(Utility_1);
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
            return _this;
        }
        DirectSpError.convert = function (data) {
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
                error.errorName = Utility_1.default.checkUndefined(data.errorName, null);
                error.errorNumber = Utility_1.default.checkUndefined(data.errorNumber, null);
                error.errorMessage = Utility_1.default.checkUndefined(data.errorMessage, null);
                error.errorType = Utility_1.default.checkUndefined(data.errorType, null);
                error.errorDescription = Utility_1.default.checkUndefined(data.errorDescription, null);
                error.errorProcName = Utility_1.default.checkUndefined(data.errorProcName, null);
                error.status = Utility_1.default.checkUndefined(data.status, null);
                error.statusText = Utility_1.default.checkUndefined(data.statusText, null);
                error.innerError = Utility_1.default.checkUndefined(data.innerError, null);
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
                        return DirectSpError.convert(obj);
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
    exports.default = DirectSpError;
});
define("DirectSpHtmlStorage", ["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    var DirectSpHtmlStorage = /** @class */ (function () {
        function DirectSpHtmlStorage(storage) {
            this.storage = storage;
        }
        DirectSpHtmlStorage.prototype.getItem = function (key) {
            var _this = this;
            return new Promise(function (resolve) {
                resolve(_this.storage.getItem(key));
            });
        };
        DirectSpHtmlStorage.prototype.setItem = function (key, value) {
            var _this = this;
            return new Promise(function (resolve) {
                _this.storage.setItem(key, value);
                resolve();
            });
        };
        DirectSpHtmlStorage.prototype.removeItem = function (key) {
            var _this = this;
            return new Promise(function (resolve) {
                _this.storage.removeItem(key);
                resolve();
            });
        };
        return DirectSpHtmlStorage;
    }());
    exports.default = DirectSpHtmlStorage;
});
define("util/Html", ["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
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
    exports.default = Html;
});
define("util/Uri", ["require", "exports"], function (require, exports) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
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
    exports.default = Uri;
});
define("dspClient", ["require", "exports", "DirectSpHtmlStorage", "DirectSpError", "util/Html", "util/Uri", "util/Convert", "util/Utility"], function (require, exports, DirectSpHtmlStorage_1, DirectSpError_1, Html_1, Uri_1, Convert_2, Utility_2) {
    "use strict";
    Object.defineProperty(exports, "__esModule", { value: true });
    DirectSpHtmlStorage_1 = __importDefault(DirectSpHtmlStorage_1);
    DirectSpError_1 = __importDefault(DirectSpError_1);
    Html_1 = __importDefault(Html_1);
    Uri_1 = __importDefault(Uri_1);
    Convert_2 = __importDefault(Convert_2);
    Utility_2 = __importDefault(Utility_2);
    var directSp;
    (function (directSp) {
        directSp.DirectSpHtmlStorage = DirectSpHtmlStorage_1.default;
        directSp.DirectSpError = DirectSpError_1.default;
        directSp.Html = Html_1.default;
        directSp.Uri = Uri_1.default;
        directSp.Convert = Convert_2.default;
        directSp.Utility = Utility_2.default;
    })(directSp = exports.directSp || (exports.directSp = {}));
    window.directSp = directSp;
});
