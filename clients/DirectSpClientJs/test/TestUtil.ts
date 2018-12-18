import { directSp } from "../lib/directsp.js";

type FetchCallback = (request: directSp.IDirectSpRequest) => directSp.IDirectSpResponse;

class DirectSpAjaxProviderFake implements directSp.IDirectSpAjaxProvider {
  private _fetch: FetchCallback | null;

  public constructor(fetch: FetchCallback | null) {
    this._fetch = fetch;
  }

  public fetch(request: directSp.IDirectSpRequest): Promise<directSp.IDirectSpResponse> {
    const res = this._fetch ? this._fetch(request) : { data: {} };
    if (res.data && !(res.data instanceof String))
      res.data = JSON.stringify(res.data);
    return Promise.resolve(res);
  }
}

export class DirectSpControlFake implements directSp.IDirectSpControl {
  public location: URL | null = null;
  public called_request: boolean = false;
  public called_navigateUri: string | null = null;
  public called_downloadUri: string | null = null;

  constructor(location: URL | null) {
    this.location = location;
  }

  public reload(): Promise<void> {
    this.called_request = true;
    return Promise.resolve();
  }
  public navigate(uri: string): Promise<void> {
    this.called_navigateUri = uri;
    return Promise.resolve();
  }
  public download(uri: string): Promise<void> {
    this.called_downloadUri = uri;
    return Promise.resolve();
  }
}

export interface ICreateDspClientOptions {
  originalUri?: URL | null;
}

export class TestUtil {
  static CreateDspClient(fetch: FetchCallback | null, options: directSp.IDirectSpOptions = {}, fakeOptions: ICreateDspClientOptions = {}): directSp.DirectSpClient {

    fakeOptions.originalUri = directSp.Utility.checkUndefined(fakeOptions.originalUri, null);

    options.resourceApiUri = directSp.Utility.checkUndefined(options.resourceApiUri, "https://fake_server.local");
    options.ajaxProvider = directSp.Utility.checkUndefined(options.ajaxProvider, new DirectSpAjaxProviderFake(fetch));
    options.dspLocalStorage = directSp.Utility.checkUndefined(options.dspLocalStorage, new directSp.DirectSpMemStorage());
    options.dspSessionStorage = directSp.Utility.checkUndefined(options.dspSessionStorage, new directSp.DirectSpMemStorage());
    options.isLogEnabled = directSp.Utility.checkUndefined(options.isLogEnabled, false);
    options.control = directSp.Utility.checkUndefined(options.control, new DirectSpControlFake(fakeOptions.originalUri));

    const client = new directSp.DirectSpClient(options);
    return client;
  }
}