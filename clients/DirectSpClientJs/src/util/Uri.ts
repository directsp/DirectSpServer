
export default class Uri {

    public static getParameterByName = function (name: string, url: string): string | null {
        if (!url && window && window.location && window.location.href) url = window.location.href;

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
