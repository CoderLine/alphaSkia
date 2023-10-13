import * as fs from 'fs';
import * as path from 'path';
import { AlphaSkiaImage, AlphaSkiaTextBaseline, AlphaSkiaTextAlign, AlphaSkiaCanvas, AlphaSkiaTypeface, addSearchPaths } from "src/alphaskia";

export type RenderFunction = (canvas: AlphaSkiaCanvas) => AlphaSkiaImage;

export class MusicSheetRenderTest {
    #musicTypeface!: AlphaSkiaTypeface;

    public get musicTypeface() { return this.#musicTypeface; }
    public renderScale = 1;
    public musicFontSize = 34;

    #customTypefaces: Map<string, AlphaSkiaTypeface | undefined> = new Map<string, AlphaSkiaTypeface | undefined>();

    private customTypefaceKey(fontFamily: string, isBold: boolean, isItalic: boolean): string {
        return fontFamily.toLowerCase() + "_" + isBold + "_" + isItalic;
    }


    public before() {
        addSearchPaths(path.join(this.#repositoryRoot, 'dist'));

        const bravura = this.#readFont("font/bravura/Bravura.ttf");

        this.#musicTypeface = AlphaSkiaTypeface.register(bravura)!;

        this.#customTypefaces.set(this.customTypefaceKey("Roboto", false, false),
            AlphaSkiaTypeface.register(this.#readFont("font/roboto/Roboto-Regular.ttf")));
        this.#customTypefaces.set(this.customTypefaceKey("Roboto", true, false),
            AlphaSkiaTypeface.register(this.#readFont("font/roboto/Roboto-Bold.ttf")));
        this.#customTypefaces.set(this.customTypefaceKey("Roboto", false, true),
            AlphaSkiaTypeface.register(this.#readFont("font/roboto/Roboto-Italic.ttf")));
        this.#customTypefaces.set(this.customTypefaceKey("Roboto", true, true),
            AlphaSkiaTypeface.register(this.#readFont("font/roboto/Roboto-BoldItalic.ttf")));

        this.#customTypefaces.set(this.customTypefaceKey("PT Serif", false, false),
            AlphaSkiaTypeface.register(this.#readFont("font/ptserif/PTSerif-Regular.ttf")));
        this.#customTypefaces.set(this.customTypefaceKey("PT Serif", true, false),
            AlphaSkiaTypeface.register(this.#readFont("font/ptserif/PTSerif-Bold.ttf")));
        this.#customTypefaces.set(this.customTypefaceKey("PT Serif", false, true),
            AlphaSkiaTypeface.register(this.#readFont("font/ptserif/PTSerif-Italic.ttf")));
        this.#customTypefaces.set(this.customTypefaceKey("PT Serif", true, true),
            AlphaSkiaTypeface.register(this.#readFont("font/ptserif/PTSerif-BoldItalic.ttf")));
    }

    public getTypeface(name: string, isBold: boolean, isItalic: boolean): AlphaSkiaTypeface {
        const typeface = this.#customTypefaces.get(this.customTypefaceKey(name, isBold, isItalic));
        if (!typeface) {
            throw new Error("Unknown font requested: " + name);
        }
        return typeface;
    }

    public textAlign: AlphaSkiaTextAlign = AlphaSkiaTextAlign.Center;
    public textBaseline: AlphaSkiaTextBaseline = AlphaSkiaTextBaseline.Top;
    public typeface!: AlphaSkiaTypeface;
    public fontSize: number = 0;
    public totalWidth: number = 0;
    public totalHeight: number = 0;
    public partPositions: number[][] = [];
    public allParts: RenderFunction[] = [];

    #readFont(relativePath: string): ArrayBuffer {
        const fullPath = path.join(this.#testDataPath, relativePath);
        return fs.readFileSync(fullPath).buffer as ArrayBuffer;
    }

    get #testDataPath(): string {
        return path.join(this.#repositoryRoot, 'lib', 'test');
    }

    #_repositoryRoot: string | undefined;
    get #repositoryRoot(): string {
        if (!this.#_repositoryRoot) {
            this.#_repositoryRoot = this.#findRepositoryRoot(path.resolve('.'));
        }
        return this.#_repositoryRoot!;
    }


    #findRepositoryRoot(dir: string): string {
        if (fs.existsSync(path.join(dir, '.nuke'))) {
            return dir;
        }

        const parent = path.resolve(dir, '..');
        if (parent == dir) {
            throw new Error("could not find repository root");
        }

        return this.#findRepositoryRoot(parent);
    }

    #renderFullImage(): AlphaSkiaImage {
        using canvas = new AlphaSkiaCanvas();

        var images = this.allParts.map(p => p(canvas));

        canvas.beginRender(this.totalWidth, this.totalHeight, this.renderScale);

        for (var i = 0; i < images.length; i++) {
            canvas.drawImage(images[i], this.partPositions[i][0], this.partPositions[i][1], images[i].width / this.renderScale, images[i].height / this.renderScale);
            images[i][Symbol.dispose]();
        }

        return canvas.endRender()!;
    }


    public render(fileName: string) {
        using finalImage = this.#renderFullImage();
        const outputPath = path.join(this.#repositoryRoot, 'lib', 'node', 'alphaskia', 'test-outputs');
        fs.mkdirSync(outputPath, { recursive: true });
        fs.writeFileSync(path.join(outputPath, fileName + '.png'),
            new Uint8Array(finalImage.toPng()!)
        );
    }
}