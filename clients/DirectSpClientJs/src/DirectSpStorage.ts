import { IDirectSpKeyToAny } from "./DirectSpUtil";

export interface IDirectSpStorage {
    getItem(key: string): Promise<string | null>;
    setItem(key: string, value: string): Promise<void>;
    removeItem(key: string): Promise<void>;
}

export class DirectSpHtmlStorage implements IDirectSpStorage {
    private _storage: Storage;

    constructor(storage: Storage) {
        this._storage = storage;
    }

    public getItem(key: string): Promise<string | null> {
        return new Promise((resolve: any) => {
            resolve(this._storage.getItem(key));
        });
    }

    public setItem(key: string, value: string): Promise<void> {
        return new Promise((resolve) => {
            this._storage.setItem(key, value);
            resolve();
        });
    }

    public removeItem(key: string): Promise<void> {
        return new Promise((resolve) => {
            this._storage.removeItem(key);
            resolve();
        });
    }
}

export class DirectSpMemStorage implements IDirectSpStorage {
    private _data: IDirectSpKeyToAny = {};
    
    public getItem(key: string): Promise<string> {
        return Promise.resolve(this._data[key]);
    }
    
    public setItem(key: string, value: string): Promise<void> {
        this._data[key] = value;
        return Promise.resolve();
    }
    
    public removeItem(key: string): Promise<void> {
        delete this._data[key];
        return Promise.resolve();
    }
}
