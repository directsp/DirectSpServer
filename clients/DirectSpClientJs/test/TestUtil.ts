import { DirectSpClient } from "../src/DirectSpClient";
import { DirectSpMemStorage } from "../src/DirectSpStorage";
import { IDirectSpAjaxProvider, IDirectSpRequest, IDirectSpResponse } from "../src/DirectSpAjax";

export type FetchCallback = (request: IDirectSpRequest) => IDirectSpResponse;

export class fetchProvider implements IDirectSpAjaxProvider {
  private _fetch: FetchCallback;

  public constructor(fetch: FetchCallback) {
    this._fetch = fetch;
  }

  public fetch(request: IDirectSpRequest): Promise<IDirectSpResponse> {
    const res = this._fetch(request);
    if (res.data && !(res.data instanceof String))
      res.data = JSON.stringify(res.data);
    return Promise.resolve(res);
  }
}

export class TestUtil {
  static CreateDspClient(fetch: FetchCallback): DirectSpClient {
    const client = new DirectSpClient({
      resourceApiUri: "https://fakeserver.local",
      ajaxProvider: new fetchProvider(fetch),
      dspLocalStorage: new DirectSpMemStorage(),
      dspSessionStorage: new DirectSpMemStorage(),
      isLogEnabled: false
    });
    return client;
  }
}