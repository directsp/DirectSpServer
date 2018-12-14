export class Convert {

    public static toBoolean(value: string | number | undefined | null | boolean, defaultValue: boolean = false): boolean {
        if (value === null || value === undefined)
            return defaultValue;

        // check is value boolean
        if (typeof value == "boolean")
            return value;

        if (typeof value == "number")
            return value != 0;

        if (typeof value == "string") {
            let parsed = parseInt(value);
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

        throw new Error(`Could not convert ${typeof value} to boolean!`);
    };

    public static toInteger(value: any, defaultValue: number = 0): number {
        if (value === null || value === undefined)
            return defaultValue;

        return parseInt(value);
    };

    public static toQueryString(obj: any): string {
        let parts = [];
        for (let key in obj) {
            if (obj.hasOwnProperty(key) && obj[key]!=null) {
                parts.push(encodeURIComponent(key) + "=" + encodeURIComponent(obj[key]));
            }
        }
        return parts.join("&");
    };

    public static buffer_fromBase64(base64String: string): string {
        if (Utility.isHtmlHost && window.atob)
            return window.atob(base64String);

        var e: any = {},
            i,
            b = 0,
            c,
            x,
            l = 0,
            a,
            r = "",
            w = String.fromCharCode,
            L = base64String.length;
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
}


export class Html {
    
    public static submit(url: string, params: any) {
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
    }
}


export class Uri {

    public static getParameterByName = function (name: string, url: string | null ): string | null {
        if (!url) return null;

        name = name.replace(/[\[\]]/g, "\\$&");
        let regex = new RegExp("[#?&]" + name + "(=([^&#]*)|&|#|$)"),
            results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return "";
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    };

    public static combine = function (uriBase: string, uriRelative: string): string {
        let parts = [uriBase, uriRelative];
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

    public static getParent = function (uri: string): string {
        let endIndex = uri.indexOf("?");
        if (endIndex == -1) endIndex = uri.length;
        let lastSlash = uri.lastIndexOf("/", endIndex);
        return uri.substr(0, lastSlash);
    };

    public static getUrlWithoutQueryString(uri: string): string {
        let i: number = uri.indexOf("?");
        if (i != -1)
            uri = uri.slice(0, i);

        i = uri.indexOf("#");
        if (i != -1)
            uri = uri.slice(i + 1);

        return uri;
    };

    public static getFileName(uri: string): string | null {
        uri = Uri.getUrlWithoutQueryString(uri);
        let i: number = uri.indexOf("/");
        if (i != -1)
            uri = uri.slice(i + 1);
        return uri;
    };
}

export class Utility {

    public static checkUndefined<T>(value: T | undefined, defValue: T): T {
        return value !== undefined ? value : defValue;
    };

    public static parseJwt(token: string): any {
        let base64Url = token.split(".")[1];
        let base64 = base64Url.replace("-", "+").replace("_", "/");
        return JSON.parse(Convert.buffer_fromBase64(base64));
    };

    public static clone<T>(obj: T): T {
        if (obj == null) return obj;
        return JSON.parse(JSON.stringify(obj));
    };

    public static getRandomInt(min: number, max: number): number {
        min = Math.ceil(min);
        max = Math.floor(max);
        return Math.floor(Math.random() * (max - min)) + min;
    };

    public static generateGuid(): string {
        return Math.random()
            .toString(36)
            .substring(2) + new Date().getTime().toString(36);
    };

    public static toCamelcase(str: string): string {
        if (str == null || str == "") return str;
        return str.substr(0, 1).toLowerCase() + str.substr(1);
    };

    //return null if error occur
    public static tryParseJason(json: string): any {
        try {
            return JSON.parse(json);
        } catch  {
            return null;
        }
    };

    public static get isHtmlHost(): boolean {
        return typeof (window) != "undefined" && window.localStorage != null;
    }
}