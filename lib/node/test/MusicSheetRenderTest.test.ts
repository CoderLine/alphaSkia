import { AlphaSkiaCanvas } from "src/AlphaSkiaCanvas";
import { AlphaSkiaTypeface } from "src/AlphaSkiaTypeface";
import { AlphaSkiaBaseline, AlphaSkiaTextAlign } from "src/addon";
import * as fs from 'fs';

export class MusicSheetRenderTest {
    static setup() {
    }
}

describe('Simple', function () {
    it('Run', function () {
        using canvas = new AlphaSkiaCanvas();

        canvas.beginRender(800, 600);

        using arial = AlphaSkiaTypeface.create("Arial", false, false)!;
        canvas.fillText("Test", arial, 15, 20, 20, AlphaSkiaTextAlign.Left, AlphaSkiaBaseline.Top);

        using image = canvas.endRender()!;
        const png = image.toPng()!;
        fs.writeFileSync('test.png', new Uint8Array(png));
    });
})