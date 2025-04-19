import * as fs from 'fs';
import * as path from 'path';
import * as os from 'os';

import { AlphaSkiaCanvas, AlphaSkiaImage, AlphaSkiaTextStyle } from '@coderline/alphaskia';

import TestBase from './AlphaTabGeneratedRenderTestBase'
import Test from './AlphaTabGeneratedRenderTest'
import { match } from 'PixelMatch';


function findRepositoryRoot(current: string): string {
    if (fs.existsSync(path.join(current, ".nuke"))) {
        return current;
    }

    const parent = path.resolve(current, '..');
    if (parent == current) {
        throw new Error("could not find repository root");
    }

    return findRepositoryRoot(parent);
}

function renderFullImage(): AlphaSkiaImage {
    using fullImageCanvas = new AlphaSkiaCanvas();
    console.log(`Begin render with size ${Test.totalWidth}x${Test.totalHeight} at scale ${TestBase.renderScale}`);
    fullImageCanvas.beginRender(Test.totalWidth, Test.totalHeight, TestBase.renderScale);

    using partialCanvas = new AlphaSkiaCanvas();
    for (let i = 0; i < Test.allParts.length; i++) {
        console.log(`Render part ${i}`);
        using part = Test.allParts[i](partialCanvas)!;

        const x = Test.partPositions[i][0];
        const y = Test.partPositions[i][1];
        const w = Test.partPositions[i][2];
        const h = Test.partPositions[i][3];

        console.log(`Drawing part ${i} into full image at ${x}/${y}/${w}/${h}`);
        fullImageCanvas.drawImage(part, x, y, w, h);
    }

    const fullImage = fullImageCanvas.endRender();
    console.log("Render of full image completed");
    return fullImage!;
}

let alphaSkiaTestRid: string;
switch (os.type()) {
    case "Linux":
        alphaSkiaTestRid = "linux";
        break;
    case "Darwin":
        alphaSkiaTestRid = "macos";
        break;
    case "Windows_NT":
        alphaSkiaTestRid = "win";
        break;
    default:
        throw new Error("Unsupported test platform");
}


function main(): number {
    const tolerancePercent = 1;
    try {
        const repositoryRoot = findRepositoryRoot(path.resolve('.'));
        const isFreeType = process.argv.includes("--freetype");
        if (isFreeType) {
            console.log("Switching to FreeType Fonts");
            AlphaSkiaCanvas.switchToFreeTypeFonts();
        }

        // Load all fonts for rendering
        console.log("Loading fonts");
        let testDataPath = path.join(repositoryRoot, "test", "test-data");
        const musicTypeface = TestBase.loadTypeface(path.join(testDataPath, "font", "bravura", "Bravura.otf"));
        TestBase.musicTextStyle  = new AlphaSkiaTextStyle(
            [musicTypeface.familyName],
            musicTypeface.weight,
            musicTypeface.isItalic
        );

        TestBase.loadTypeface(path.join(testDataPath, "font", "noto-sans", "NotoSans-Regular.otf"));
        TestBase.loadTypeface(path.join(testDataPath, "font", "noto-sans", "NotoSans-Bold.otf"));
        TestBase.loadTypeface(path.join(testDataPath, "font", "noto-sans", "NotoSans-Italic.otf"));
        TestBase.loadTypeface(path.join(testDataPath, "font", "noto-sans", "NotoSans-BoldItalic.otf"));
        TestBase.loadTypeface(path.join(testDataPath, "font", "noto-serif", "NotoSerif-Regular.otf"));
        TestBase.loadTypeface(path.join(testDataPath, "font", "noto-serif", "NotoSerif-Bold.otf"));
        TestBase.loadTypeface(path.join(testDataPath, "font", "noto-serif", "NotoSerif-Italic.otf"));
        TestBase.loadTypeface(path.join(testDataPath, "font", "noto-serif", "NotoSerif-BoldItalic.otf"));
        TestBase.loadTypeface(path.join(testDataPath, "font", "noto-music", "NotoMusic-Regular.otf"));
        TestBase.loadTypeface(path.join(testDataPath, "font", "noto-color-emoji", "NotoColorEmoji-Regular.ttf"));
        console.log("Fonts loaded");

        // render full image
        console.log("Rendering image");
        using actualImage = renderFullImage();
        console.log("Image rendered");

        // save png for future reference
        console.log("Saving image as PNG");

        let testOutputPath = path.join(repositoryRoot, "test", "test-outputs", "dotnet");
        fs.mkdirSync(testOutputPath, { recursive: true })

        const testOutputFileBase = isFreeType ? "freetype" : alphaSkiaTestRid;

        let testOutputFile = path.join(testOutputPath, testOutputFileBase + ".png");
        let pngData = actualImage.toPng();
        if (!pngData) {
            throw new Error("Failed to encode final image to png");
        }

        fs.writeFileSync(testOutputFile, new Uint8Array(pngData));
        console.log(`Image saved to ${testOutputFile}`);

        // load reference image
        let testReferencePath = path.join(testDataPath, "reference", testOutputFileBase + ".png");
        console.log(`Loading reference image ${testReferencePath}`);

        let testReferenceData = fs.readFileSync(testReferencePath);
        using expectedImage = AlphaSkiaImage.decode(testReferenceData.buffer);
        if (expectedImage == null) {
            throw new Error("Failed to decode reference image");
        }

        console.log("Reference image loaded");

        // compare images
        console.log("Comparing images");
        let actualWidth = actualImage.width;
        let actualHeight = actualImage.height;

        let expectedWidth = expectedImage.width;
        let expectedHeight = expectedImage.height;
        if (actualWidth != expectedWidth || actualHeight != expectedHeight) {
            console.error(
                `Image sizes do not match: Actual[${actualWidth}x${actualHeight}] Expected[${expectedWidth}x${actualHeight}]`);
            return 1;
        }

        let actualPixels = new Uint8Array(actualImage.readPixels()!);
        let expectedPixels = new Uint8Array(expectedImage.readPixels()!);
        let diffPixels = new Uint8Array(actualPixels.byteLength);

        let compareResult = match(actualPixels,
            expectedPixels,
            diffPixels,
            actualWidth,
            actualHeight,
            {
                threshold: 0.3,
                diffColor: { r: 255, g: 0, b: 0 }
            });

        let totalPixels = compareResult.totalPixels - compareResult.transparentPixels;
        let percentDifference = (compareResult.differentPixels / totalPixels) * 100;
        let pass = percentDifference < tolerancePercent;
        if (!pass) {
            console.error(`Difference between original and new image is too big: ${compareResult.differentPixels}/${totalPixels}(${percentDifference}%)`);

            // create diff image as PNG
            using diffImage = AlphaSkiaImage.fromPixels(actualWidth, actualHeight, diffPixels)!;
            let diffImagePngData = diffImage.toPng()!;

            let diffOutputPath = path.join(testOutputPath, testOutputFileBase + ".diff.png");
            fs.writeFileSync(diffOutputPath, new Uint8Array(diffImagePngData));
            console.log(`Error diff image saved to ${diffOutputPath}`);
            return 1;
        }

        console.log(`Images match. Total Pixels:${compareResult.totalPixels}, Transparent Pixels: ${compareResult.transparentPixels}, Percent difference: ${percentDifference}%`);
        return 0;
    }
    catch (e) {
        console.error(`Failed to run test`, e);
        return 1;
    }
}

process.exit(main());