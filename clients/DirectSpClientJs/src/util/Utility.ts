import Convert from "Convert"

export default class Utility {

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

    public static isHtmlHost(): boolean {
        return window && window.localStorage != null;
    }
}