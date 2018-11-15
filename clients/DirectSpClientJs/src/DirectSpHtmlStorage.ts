export default class DirectSpHtmlStorage {
    private storage: Storage;

    constructor(storage: Storage) {
        this.storage = storage;
    }

    public getItem(key: string): Promise<string | null> {
        return new Promise((resolve: any) => {
            resolve(this.storage.getItem(key));
        });
    }

    public setItem(key: string, value: string): Promise<void> {
        return new Promise((resolve) => {
            this.storage.setItem(key, value);
            resolve();
        });
    }

    public removeItem(key: string): Promise<void> {
        return new Promise((resolve) => {
            this.storage.removeItem(key);
            resolve();
        });
    }
}
