var router;

function init() {
    //init router
    initRouter();

    //dspClient.apiHook = function (method, props, options) {
    //    options.delay = 5000;
    //    console.log(method);
    //    return null;
    //};

    //init dspClient
    //dspClient.autoSignIn = true;
    dspClient.autoSignIn = false;
    dspClient.authBaseUrl = 'https://auth.google.com';
    dspClient.authEndpointUri = "https://accounts.google.com/o/oauth2/auth";
    dspClient.token_uri = "https://accounts.google.com/o/oauth2/token";
    dspClient.authType = "id_token";
    dspClient.authScope = "profile";
    dspClient.isApiLogEnabled = true;
    dspClient.onAuthorized = onAuthorized;
    dspClient.clientId = "637321499771-saico43e4keck231irbi50hmeq84831k.apps.googleusercontent.com";
    dspClient.init();

    //init handler
    $("#signin").click(signIn);
}

var paginator;
function onAuthorized(data) {
    if (!dspClient.isAuthorized) {
        return;
    }

    dspClient.invokeApi("City_AllGet", null, {})
        .done(function (data) {
            console.log(data);
        })
        .fail(function (data) {
            console.warn(data);
        });

    dspClient.help("User_PropsGet");

}

function foo() {

    if (dspClient.user != null)
        dspClient.invokeApi("System_Api", {})
            .done(function (data) {
                //console.log("OK");
                //console.log(data);
            })
            .fail(function (data) {
                //console.log("fail");
                //console.log(data);
            });
}

//init router
function initRouter() {
    router = new Navigo(root = "/app", useHash = false);
    router.on({
        //'/error': function () { showPage("error"); error_onPageLoad(); },
    }).resolve();

    router.updatePageLinks();
}

function showPage(pageName) {
    $(".ic-page").addClass("ic-hide");
    $("#" + pageName + "_page").removeClass("ic-hide");
}

function checkLogin() {
    if (!dspClient.is_authorized) {
        console.log("user has not logged in!", dspClient.user_username, dspClient.user_displayname);
    }
    else {
        console.log("user logged in: ", dspClient.user_username, dspClient.user_displayname);
        console.log(dspClient.tokens);
    }

}

function signIn(event) {
    dspClient.signIn();
}

function signOut() {
    dspClient.signOut().always(function (data) {
        console.log(dspClient.user_username);
    });
}

function login_onPageLoad() {
    console.log("login_onPageLoad");
}

function grant_onPageLoad() {
    console.log("grant_onPageLoad");
}

$(init);

