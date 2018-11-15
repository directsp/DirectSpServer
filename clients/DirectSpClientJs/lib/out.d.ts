declare module "util/Convert" {
    export default class Convert {
        static toBoolean(value: string | number | undefined | null, defaultValue?: boolean): boolean;
        static toInteger(value: any, defaultValue?: number): number;
        static toQueryString(obj: any): string;
        static buffer_fromBase64(base64String: string): string;
    }
}
declare module "util/Utility" {
    export default class Utility {
        static checkUndefined<T>(value: T | undefined, defValue: T): T;
        static parseJwt(token: string): any;
        static clone<T>(obj: T): T;
        static getRandomInt(min: number, max: number): number;
        static generateGuid(): string;
        static toCamelcase(str: string): string;
        static tryParseJason(json: string): any;
        static isHtmlHost(): boolean;
    }
}
declare module "DirectSpError" {
    export default class DirectSpError extends Error {
        errorType: string | null;
        errorName: string | null;
        errorNumber: number | null;
        errorMessage: string | null;
        errorDescription: string | null;
        errorProcName: string | null;
        status: number | null;
        statusText: string | null;
        innerError: Error | null;
        constructor(message: string);
        static convert(data: any): DirectSpError;
    }
}
declare module "DirectSpHtmlStorage" {
    export default class DirectSpHtmlStorage {
        private storage;
        constructor(storage: Storage);
        getItem(key: string): Promise<string | null>;
        setItem(key: string, value: string): Promise<void>;
        removeItem(key: string): Promise<void>;
    }
}
declare module "util/Html" {
    export default class Html {
        static submit(url: string, params: any): void;
    }
}
declare module "util/Uri" {
    export default class Uri {
        static getParameterByName: (name: string, url: string) => string | null;
        static combine: (uriBase: string, uriRelative: string) => string;
        static getParent: (uri: string) => string;
        static getUrlWithoutQueryString(uri: string): string;
        static getFileName(uri: string): string | null;
    }
}
declare module "dspClient" {
    import DirectSpHtmlStorageClass from "DirectSpHtmlStorage";
    import DirectSpErrorClass from "DirectSpError";
    import HtmlClass from "util/Html";
    import UriClass from "util/Uri";
    import ConvertClass from "util/Convert";
    import UtilityClass from "util/Utility";
    export namespace directSp {
        const DirectSpHtmlStorage: typeof DirectSpHtmlStorageClass;
        const DirectSpError: typeof DirectSpErrorClass;
        const Html: typeof HtmlClass;
        const Uri: typeof UriClass;
        const Convert: typeof ConvertClass;
        const Utility: typeof UtilityClass;
    }
}
