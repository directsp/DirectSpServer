namespace directSp {
  type FetchCallback = (request: IDirectSpRequest) => IDirectSpResponse;

  class DirectSpAjaxProviderFake implements IDirectSpAjaxProvider {
    private _fetch: FetchCallback | null;

    public constructor(fetch: FetchCallback | null) {
      this._fetch = fetch;
    }

    public fetch(request: IDirectSpRequest): Promise<IDirectSpResponse> {
      const res = this._fetch ? this._fetch(request) : { data: {} };
      if (res.data && !(res.data instanceof String))
        res.data = JSON.stringify(res.data);
      return Promise.resolve(res);
    }
  }

  export class DirectSpControlFake implements IDirectSpControl {
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
    static CreateDspClient(fetch: FetchCallback | null, options: IDirectSpOptions = {}, fakeOptions: ICreateDspClientOptions = {}): DirectSpClient {

      fakeOptions.originalUri = Utility.checkUndefined(fakeOptions.originalUri, null);

      options.resourceApiUri = Utility.checkUndefined(options.resourceApiUri, "https://fake_server.local");
      options.ajaxProvider = Utility.checkUndefined(options.ajaxProvider, new DirectSpAjaxProviderFake(fetch));
      options.dspLocalStorage = Utility.checkUndefined(options.dspLocalStorage, new DirectSpMemStorage());
      options.dspSessionStorage = Utility.checkUndefined(options.dspSessionStorage, new DirectSpMemStorage());
      options.isLogEnabled = Utility.checkUndefined(options.isLogEnabled, false);
      options.control = Utility.checkUndefined(options.control, new DirectSpControlFake(fakeOptions.originalUri));

      const client = new DirectSpClient(options);
      return client;
    }
  }
}