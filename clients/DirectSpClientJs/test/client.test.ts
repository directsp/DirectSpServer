import { TestUtil } from "./TestUtil";
import { IDirectSpCall } from "../src/DirectSpClient";
import { DirectSpError } from "../src/DirectSpError";


test('invoke and its options', async () => {

  //simulate result
  const client = TestUtil.CreateDspClient((request) => {
    if (!request.data) throw new DirectSpError("request does not contain data!");
    if (!request.headers) throw new DirectSpError("request does not contain headers!");

    expect(request.url).toBe("https://fake_server.local/test1");
    expect(request.headers["Content-Type"].toLowerCase()).toBe("application/json;charset=utf-8");
    expect(request.data.spCall.method).toBe("test1");
    expect(request.data.spCall.params.param1).toBe(1);
    expect(request.data.spCall.params.param2).toBe("param2value");
    expect(request.data.invokeOptions.recordIndex).toBe(10);

    const data = {
      item1: 1,
      item2: 2,
    }
    return { data: data }
  });

  // check request
  const res = await client.invoke("test1", { param1: 1, param2: "param2value" }, { recordIndex: 10 });
  expect(res.item1).toBe(1);
  expect(res.item2).toBe(2);
});


test('invokeBatch and its options', async () => {
  // simulate result
  const client = TestUtil.CreateDspClient((request) => {
    if (!request.data) throw new DirectSpError("request does not contain data!");

    //check invoke option
    expect(request.data.invokeOptions.captchaId).toBe("invokeBatch-captcha");

    let data = [];
    for (var i = 0; i < request.data.spCalls.length; i++) {
      let spCall = request.data.spCalls[i];
      if (spCall.method == 'concat') data[i] = { returnValue: spCall.params.param1 + spCall.params.param2 };
      else if (spCall.method == 'cross') data[i] = { returnValue: spCall.params.param1 * spCall.params.param2 };
      else throw new DirectSpError("Unknown method!");
    }

    return { data: data }
  });

  // send request
  const spCalls: IDirectSpCall[] = [
    { method: "concat", params: { param1: 2, param2: 3 } },
    { method: "cross", params: { param1: 20, param2: 30 } },
  ];

  const res = await client.invokeBatch(spCalls, { captchaId: "invokeBatch-captcha" });
  expect(res.length).toBe(2);
  expect(res[0].returnValue).toBe(5);
  expect(res[1].returnValue).toBe(600);
});


test('seqGroup option and onBeforeInvoke', async () => {
  // simulate result
  const client = TestUtil.CreateDspClient((request) => {
    if (!request.data) throw new DirectSpError("request does not contain data!");

    let data = { result: request.data.spCall.method };

    return { data: data }
  });

  client.onBeforeInvoke = (hookParams) => {
    hookParams.isRandomDelay = false;
    if (hookParams.invokeParams && hookParams.invokeParams.spCall && hookParams.invokeParams.spCall.params)
      hookParams.delay = hookParams.invokeParams.spCall.params.delay;
    return Promise.resolve();
  };

  const promises: Promise<any>[] = [];
  const results: boolean[] = [];
  promises.push(client.invoke("method1", { delay: 100 }, { seqGroup: "testGroup1" }).then(() => { results[0] = true; }).catch((e) => { results[0] = false; }));
  promises.push(client.invoke("method1", { delay: 200 }, { seqGroup: "testGroup1" }).then(() => { results[1] = true; }).catch((e) => { results[1] = false; }));
  promises.push(client.invoke("method1", { delay: 120 }, { seqGroup: "testGroup1" }).then(() => { results[2] = true; }).catch(() => { results[2] = false; }));

  promises.push(client.invoke("method4", { delay: 70 }, { seqGroup: "testGroup2" }).then(() => { results[3] = true; }).catch(() => { results[3] = false; }));
  promises.push(client.invoke("method4", { delay: 40 }, { seqGroup: "testGroup3" }).then(() => { results[4] = true; }).catch(() => { results[4] = false; }));

  await Promise.all(promises);

  expect(results[0]).toBe(false);
  expect(results[1]).toBe(false);
  expect(results[2]).toBe(true);
  expect(results[3]).toBe(true);
  expect(results[4]).toBe(true);

});
