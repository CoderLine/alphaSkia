import * as fs from 'fs';
import * as path from 'path';
import * as os from 'os';

import { AlphaSkiaCanvas, AlphaSkiaImage } from '@coderline/alphaskia';

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
        using part = Test.allParts[i](partialCanvas);

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

let operatingSystemRid;
switch (os.type()) {
    case "Linux":
        operatingSystemRid = "linux";
        break;
    case "Darwin":
        operatingSystemRid = "macos";
        break;
    case "Windows_NT":
        operatingSystemRid = "win";
        break;
    default:
        throw new Error("Unsupported test platform");
}

let architectureRid;
switch (process.arch) {
    case "arm":
        architectureRid = "arm";
        break;
    case "arm64":
        architectureRid = "arm64";
        break;
    case "x64":
        architectureRid = "x64";
        break;
    case "ia32":
        architectureRid = "x86";
        break;
    default:
        throw new Error("Unsupported test platform");
        break;
}

const alphaSkiaRid = operatingSystemRid + "-" + architectureRid;


function main(): number {
    const tolerancePercent = 1;
    try {
        const repositoryRoot = findRepositoryRoot(path.resolve('.'));

        // Load all fonts for rendering
        console.log("Loading fonts");
        let testDataPath = path.join(repositoryRoot, "test", "test-data");
        TestBase.musicTypeface = TestBase.loadTypeface("Bravura", false, false,
            path.join(testDataPath, "font", "bravura", "Bravura.ttf"));
        TestBase.loadTypeface("Roboto", false, false,
            path.join(testDataPath, "font", "roboto", "Roboto-Regular.ttf"));
        TestBase.loadTypeface("Roboto", true, false,
            path.join(testDataPath, "font", "roboto", "Roboto-Bold.ttf"));
        TestBase.loadTypeface("Roboto", false, true,
            path.join(testDataPath, "font", "roboto", "Roboto-Italic.ttf"));
        TestBase.loadTypeface("Roboto", true, true,
            path.join(testDataPath, "font", "roboto", "Roboto-BoldItalic.ttf"));
        TestBase.loadTypeface("PT Serif", false, false,
            path.join(testDataPath, "font", "ptserif", "PTSerif-Regular.ttf"));
        TestBase.loadTypeface("PT Serif", true, false,
            path.join(testDataPath, "font", "ptserif", "PTSerif-Bold.ttf"));
        TestBase.loadTypeface("PT Serif", false, true,
            path.join(testDataPath, "font", "ptserif", "PTSerif-Italic.ttf"));
        TestBase.loadTypeface("PT Serif", true, true,
            path.join(testDataPath, "font", "ptserif", "PTSerif-BoldItalic.ttf"));
        console.log("Fonts loaded");

        // render full image
        console.log("Rendering image");
        using actualImage = renderFullImage();
        console.log("Image rendered");

        // save png for future reference
        console.log("Saving image as PNG");

        let testOutputPath = path.join(repositoryRoot, "test", "test-outputs", "dotnet");
        fs.mkdirSync(testOutputPath, { recursive: true })

        let testOutputFile = path.join(testOutputPath, alphaSkiaRid + ".png");
        let pngData = actualImage.toPng();
        if (!pngData) {
            throw new Error("Failed to encode final image to png");
        }

        fs.writeFileSync(testOutputFile, new Uint8Array(pngData));
        console.log(`Image saved to ${testOutputFile}`);

        // load reference image
        let testReferencePath = path.join(testDataPath, "reference", alphaSkiaRid + ".png");
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

            let diffOutputPath = path.join(testOutputPath, alphaSkiaRid + ".diff.png");
            fs.writeFileSync(diffOutputPath, diffImagePngData);
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