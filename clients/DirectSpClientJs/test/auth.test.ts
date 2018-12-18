import { directSp } from "../lib/directsp.js";
import { DirectSpControlFake, TestUtil } from "./TestUtil";

// ---------------
// Test
// ---------------
test('autoSignin on', async () => {
  //init
  const client = TestUtil.CreateDspClient(null, {
    auth: {
      baseEndpointUri: "https://fake_auth_server.local/", isAutoSignIn: true,
      clientId: "1234567",
      scope: "offline_access profile",
      type: "token",
      redirectUri: "https://fakeclient.local/redirect"
    }
  });
  await client.init();

  // wait for calling onAuthorize
  await new Promise(resolve => setTimeout(resolve, 10))

  if (!(<DirectSpControlFake>client.control).called_navigateUri)
    throw new directSp.DirectSpError("SignIn have not called!");

  const url = new URL(<string>(<DirectSpControlFake>client.control).called_navigateUri);
  expect(url.origin + url.pathname).toBe("https://fake_auth_server.local/connect/authorize");
  expect(url.searchParams.get("client_id")).toBe("1234567");
  expect(url.searchParams.get("scope")).toBe("offline_access profile");
  expect(url.searchParams.get("scope")).toBe("offline_access profile");
  expect(url.searchParams.get("response_type")).toBe("token");
  expect(url.searchParams.get("state")).toBeDefined();
  expect(url.searchParams.get("redirect_uri")).toBe("https://fakeclient.local/redirect");

});

// ---------------
// Test
// ---------------
test('autoSignin off', async () => {
  //simulate result
  const client = TestUtil.CreateDspClient(null, { auth: { baseEndpointUri: "https://fakeauth.local/", isAutoSignIn: false } });
  await client.init();

  // wait for calling onAuthorize
  await new Promise(resolve => setTimeout(resolve, 10))
  expect((<DirectSpControlFake>client.control).called_navigateUri).toBe(null);
});


// ---------------
// Test
// ---------------
test('login by redirect and refresh token', async () => {
  //init options
  const options: directSp.IDirectSpOptions = {
    auth: {
      baseEndpointUri: "https://fake_auth_server.local/",
      isAutoSignIn: true,
      clientId: "1234567",
      scope: "offline_access profile",
      type: "token",
      redirectUri: "https://fakeclient.local/temp/callback",
    },
    sessionState: "fake_state",
    isLogEnabled: true
  }

  // null protection
  if (!options.auth) throw new directSp.DirectSpError("options.auth is not initialized!");

  const refresh_token1 = "fake_refresh_token1";
  const refresh_token2 = "fake_refresh_token2";

  //simulate result
  const client = TestUtil.CreateDspClient((request) => {

    //login
    if (request.data && request.data.grant_type == "authorization_code") {
      if (!request.data) throw new directSp.DirectSpError("request does not contain data!");
      if (!request.headers) throw new directSp.DirectSpError("headers does not contain data!");

      expect(request.url).toBe('https://fake_auth_server.local/connect/token');
      expect(request.headers["Content-Type"].toLowerCase()).toBe('application/x-www-form-urlencoded;charset=utf-8');
      expect(request.data.client_id).toBe('1234567');
      expect(request.data.redirect_uri).toBe('https://fakeclient.local/temp/callback');
      expect(request.data.code).toBe('fake_code');

      //return expired token to make sure refresh token is used
      const tokens: directSp.IToken = {
        access_token: "eyJhbGciOiJSUzI1NiIsImtpZCI6Ijc3NUE5MjkyRjE5RjE0RkMyQjlCODBBQjA2MkEwNkJFODYyMzYxOUMiLCJ0eXAiOiJKV1QifQ.eyJzdWIiOiIxMDAwMTIxNSIsInVzZXJuYW1lIjoiTG95YWx0eV9BZG1pbiIsInRva2VuX3VzYWdlIjoiYWNjZXNzX3Rva2VuIiwianRpIjoiNDZjNzhkMzUtNDQ0OC00NTM4LTlmNGEtZjY4ZGYzZDhjNmM5IiwiY2ZkX2x2bCI6InByaXZhdGUiLCJzY29wZSI6WyJwcm9maWxlIiwib2ZmbGluZV9hY2Nlc3MiLCIyOSJdLCJhdWQiOiJhZG1pbmxveWFsdHlfYTUwMjI1ZjAxN2ZiNDYxMzliOTc4MGU5ODdmMGJhZWEiLCJhenAiOiJhZG1pbmxveWFsdHlfYTUwMjI1ZjAxN2ZiNDYxMzliOTc4MGU5ODdmMGJhZWEiLCJuYmYiOjE1NDQxMzIxMTksImV4cCI6MTU0NDEzMzkxOSwiaWF0IjoxNTQ0MTMyMTE5LCJpc3MiOiJodHRwczovL2F1dGguaXJhbmlhbi5jYXJkcy8iLCJhY3RvcnQiOiJleUpoYkdjaU9pSnViMjVsSWl3aWRIbHdJam9pU2xkVUluMC5leUp6ZFdJaU9pSmhaRzFwYm14dmVXRnNkSGxmWVRVd01qSTFaakF4TjJaaU5EWXhNemxpT1RjNE1HVTVPRGRtTUdKaFpXRWlmUS4ifQ.fakesign",
        expires_in: 1800,
        refresh_token: refresh_token1,
        token_type: "Bearer"
      };
      return { data: tokens };
    }
    else if (request.data && request.data.grant_type == "refresh_token") {
      if (!request.data) throw new directSp.DirectSpError("request does not contain data!");
      if (!request.headers) throw new directSp.DirectSpError("headers does not contain data!");

      expect(request.url).toBe('https://fake_auth_server.local/connect/token');
      expect(request.headers["Content-Type"].toLowerCase()).toBe('application/x-www-form-urlencoded;charset=utf-8');
      expect(request.data.client_id).toBe('1234567');
      expect(request.data.refresh_token).toBe(refresh_token1);

      const tokens: directSp.IToken = {
        access_token: "eyJhbGciOiJSUzUxMiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJodHRwczovL2p3dC1pZHAuZXhhbXBsZS5jb20iLCJzdWIiOiJtYWlsdG86bWlrZUBleGFtcGxlLmNvbSIsIm5iZiI6MTU0NDE0NTQ3NywiZXhwIjoxOTE0MTkxOTk5LCJpYXQiOjE5MTQxOTE5OTksImp0aSI6ImlkMTIzNDU2IiwidHlwIjoiaHR0cHM6Ly9leGFtcGxlLmNvbS9yZWdpc3RlciJ9.fakesign",
        expires_in: 1800,
        refresh_token: refresh_token2,
        token_type: "Bearer"
      };
      return { data: tokens };
    }
    else if (request.url = "https://fake_auth_server.local/connect/userinfo") {
      return { data: { name: "David", username: "david_username" } };
    }
    else {
      throw new directSp.DirectSpError("Unexpected request!");
    }

  }, options, { originalUri: new URL(`https://fakeclient.local/temp/callback?response_type=code&code=fake_code&client_id=${options.auth.clientId}&redirect_uri=${options.auth.redirectUri}&state=${options.sessionState}&scope=${options.auth.scope}`) });

  //null protection
  if (!client.auth) throw new directSp.DirectSpError("client.auth is not initialized!");
  if (!client.auth.authRequest) throw new directSp.DirectSpError("client.auth.authRequest is not initialized!");

  //init
  await client.init();
  expect(client.auth.authRequest.client_id).toBe(options.auth.clientId);
  expect(client.auth.authRequest.redirect_uri).toBe(options.auth.redirectUri);
  expect(client.auth.authRequest.response_type).toBe("code");
  expect(client.auth.authRequest.scope).toBe(options.auth.scope);
  expect(client.auth.authRequest.state).toBe(options.sessionState);
  expect(client.auth.isAuthRequest).toBe(true);
  expect(client.auth.isAuthorized).toBe(true);
  expect(client.auth!.tokens!.refresh_token).toBe(refresh_token2); //token should be refreshed

  if (!client.auth.userInfo) throw new directSp.DirectSpError("client.auth.userInfo has not been retrieved!");
  expect(client.auth.userInfo.name).toBe("David");

});
