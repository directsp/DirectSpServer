import { DirectSpError } from "./DirectSpError";

export interface IDirectSpControl {
    readonly location: URL | null;
    reload(): Promise<void>;
    navigate(uri: string): Promise<void>;
    download(uri: string): Promise<void>;
}

export class DirectSpControlHtml implements IDirectSpControl {
    public get location(): URL | null {
        return new URL(window.location.href);
    }

    public reload(): Promise<void> {
        window.location.reload();
        return Promise.resolve();
    }

    public navigate(uri: string): Promise<void> {
        window.location.href = uri;
        return Promise.resolve();
    }

    public download(uri: string): Promise<void> {
        window.location.href = uri;
        return Promise.resolve();
    }
}

export class DirectSpControlNotImplemented implements IDirectSpControl {
    public get location(): URL | null {
        return null;
    }

    public reload(): Promise<void> {
        throw new DirectSpError("reload has not been implemeted in control!");
    }

    public navigate(uri: string): Promise<void> {
        throw new DirectSpError("navigate has not been implemeted in control!");
    }

    public download(uri: string): Promise<void> {
        throw new DirectSpError("download has not been implemeted in control!");
    }
}