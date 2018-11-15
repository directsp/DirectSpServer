
export default class Convert {

    public static toBoolean(value: string | number | undefined | null, defaultValue: boolean = false): boolean {
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
            if (obj.hasOwnProperty(key)) {
                parts.push(encodeURIComponent(key) + "=" + encodeURIComponent(obj[key]));
            }
        }
        return parts.join("&");
    };

    public static buffer_fromBase64(base64String: string): string {
        if (window.atob)
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
