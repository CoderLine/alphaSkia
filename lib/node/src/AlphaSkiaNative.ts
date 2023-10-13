export class AlphaSkiaNative<THandle> implements Disposable{
    #release: (handle: THandle) => void;
    #handle: THandle | undefined;

    /**
     * @internal
     */
    public get handle(): THandle | undefined {
        return this.#handle;
    }

    /**
     * @internal
     */
    constructor(handle: THandle, release: ((handle: THandle) => void)) {
        this.#handle = handle;
        this.#release = release;
    }

    protected checkDisposed() {
        if (!this.#handle) {
            throw new ReferenceError("Object was already disposed");
        }
    }

    [Symbol.dispose]() {
        if (this.#handle) {
            this.#release(this.#handle);
            this.#handle = undefined;
        }
    }
}
