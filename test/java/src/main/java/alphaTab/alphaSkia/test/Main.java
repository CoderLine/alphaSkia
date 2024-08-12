package alphaTab.alphaSkia.test;

import alphaTab.alphaSkia.*;

import java.io.IOException;
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;

import java.util.*; 
import java.util.stream.*; 


public class Main {
    private static Path findRepositoryRoot(Path current) {
        if (current.resolve(".nuke").toFile().exists()) {
            return current;
        }

        var parent = current.getParent();
        if (parent == null) {
            throw new IllegalStateException("Could not find repository root");
        }

        return findRepositoryRoot(parent);
    }

    private static AlphaSkiaImage renderFullImage() {
        try (var fullImageCanvas = new AlphaSkiaCanvas()) {
            System.out.println(
                    "Begin render with size " + AlphaTabGeneratedRenderTest.getTotalWidth() + "x" + AlphaTabGeneratedRenderTest.getTotalHeight() + " at scale " + AlphaTabGeneratedRenderTest.getRenderScale());
            fullImageCanvas.beginRender(AlphaTabGeneratedRenderTest.getTotalWidth(), AlphaTabGeneratedRenderTest.getTotalHeight(),
                    AlphaTabGeneratedRenderTest.getRenderScale());

            try (var partialCanvas = new AlphaSkiaCanvas()) {
                var allParts = AlphaTabGeneratedRenderTest.getAllParts();
                var partPositions = AlphaTabGeneratedRenderTest.getPartPositions();
                for (var i = 0; i < allParts.length; i++) {
                    System.out.println("Render part " + i);
                    try (var part = allParts[i].render(partialCanvas)) {
                        float x = partPositions[i][0];
                        float y = partPositions[i][1];
                        float w = partPositions[i][2];
                        float h = partPositions[i][3];

                        System.out.println("Drawing part " + i + " into full image at " + x + "/" + y + "/" + w + "/" + h);
                        fullImageCanvas.drawImage(part, x, y, w, h);
                    }
                }
            }

            var fullImage = fullImageCanvas.endRender();
            System.out.println("Render of full image completed");
            return fullImage;
        }
    }

    public static void main(String[] args) {
        System.exit(mainWithExitCode(args));
    }

    private static int mainWithExitCode(String[] args) {
        try {
            initializeAlphaSkia();
            boolean isFreeType = Arrays.stream(args).anyMatch("--freetype"::equals);
            if(isFreeType) {
                System.out.println("Switching to FreeType Fonts");
                AlphaSkiaCanvas.switchToFreeTypeFonts();
            }

            double tolerancePercent = 1;

            var repositoryRoot = findRepositoryRoot(Paths.get(".").toAbsolutePath());

            // Load all fonts for rendering
            System.out.println("Loading fonts");
            var testDataPath = repositoryRoot.resolve("test/test-data");
            AlphaTabGeneratedRenderTest.setMusicTypeface(AlphaTabGeneratedRenderTest.loadTypeface("Bravura", false, false,
                    testDataPath.resolve("font/bravura/Bravura.ttf")));
            AlphaTabGeneratedRenderTest.loadTypeface("Roboto", false, false,
                    testDataPath.resolve("font/roboto/Roboto-Regular.ttf"));
            AlphaTabGeneratedRenderTest.loadTypeface("Roboto", true, false,
                    testDataPath.resolve("font/roboto/Roboto-Bold.ttf"));
            AlphaTabGeneratedRenderTest.loadTypeface("Roboto", false, true,
                    testDataPath.resolve("font/roboto/Roboto-Italic.ttf"));
            AlphaTabGeneratedRenderTest.loadTypeface("Roboto", true, true,
                    testDataPath.resolve("font/roboto/Roboto-BoldItalic.ttf"));
            AlphaTabGeneratedRenderTest.loadTypeface("PT Serif", false, false,
                    testDataPath.resolve("font/ptserif/PTSerif-Regular.ttf"));
            AlphaTabGeneratedRenderTest.loadTypeface("PT Serif", true, false,
                    testDataPath.resolve("font/ptserif/PTSerif-Bold.ttf"));
            AlphaTabGeneratedRenderTest.loadTypeface("PT Serif", false, true,
                    testDataPath.resolve("font/ptserif/PTSerif-Italic.ttf"));
            AlphaTabGeneratedRenderTest.loadTypeface("PT Serif", true, true,
                    testDataPath.resolve("font/ptserif/PTSerif-BoldItalic.ttf"));
            System.out.println("Fonts loaded");

            // render full image
            System.out.println("Rendering image");
            try (var actualImage = renderFullImage()) {
                System.out.println("Image rendered");

                // save png for future reference
                System.out.println("Saving image as PNG");

                var testOutputPath = repositoryRoot.resolve("test/test-outputs/java");
                //noinspection ResultOfMethodCallIgnored
                testOutputPath.toFile().mkdirs();

                var testOutputFileBase = isFreeType ? "freetype" : getAlphaSkiaTestRid();

                var testOutputFile = testOutputPath.resolve(testOutputFileBase + ".png");
                var pngData = actualImage.toPng();
                if (pngData == null) {
                    throw new IllegalStateException("Failed to encode final image to png");
                }

                Files.write(testOutputFile, pngData);
                System.out.println("Image saved to " + testOutputFile);

                // load reference image
                var testReferencePath = testDataPath.resolve("reference/" + testOutputFileBase + ".png");
                System.out.println("Loading reference image " + testReferencePath);

                var testReferenceData = Files.readAllBytes(testReferencePath);
                try (var expectedImage = AlphaSkiaImage.decode(testReferenceData)) {
                    if (expectedImage == null) {
                        throw new IllegalStateException("Failed to decode reference image");
                    }

                    System.out.println("Reference image loaded");

                    // compare images
                    System.out.println("Comparing images");
                    var actualWidth = actualImage.getWidth();
                    var actualHeight = actualImage.getHeight();

                    var expectedWidth = expectedImage.getWidth();
                    var expectedHeight = expectedImage.getHeight();
                    if (actualWidth != expectedWidth || actualHeight != expectedHeight) {
                        System.err.println(
                                "Image sizes do not match: Actual[" + actualWidth + "x" + actualHeight + "] Expected[" + expectedWidth + "x" + actualHeight + "]");
                        return 1;
                    }

                    var actualPixels = actualImage.readPixels();
                    var expectedPixels = expectedImage.readPixels();
                    var diffPixels = new byte[actualPixels.length];

                    var compareResult = PixelMatch.match(actualPixels,
                            expectedPixels,
                            diffPixels,
                            actualWidth,
                            actualHeight,
                            new PixelMatchOptions(0.3, new PixelMatchColor((byte) 255, (byte) 0, (byte) 0))
                    );

                    var totalPixels = compareResult.totalPixels() - compareResult.transparentPixels();
                    var percentDifference = ((double) compareResult.differentPixels() / totalPixels) * 100;
                    var pass = percentDifference < tolerancePercent;
                    if (!pass) {
                        System.err.println(
                                "Difference between original and new image is too big: " + compareResult.differentPixels() + "/" + totalPixels + "(" + percentDifference + "%)");

                        // create diff image as PNG
                        try (var diffImage = AlphaSkiaImage.fromPixels(actualWidth, actualHeight, diffPixels)) {
                            var diffImagePngData = diffImage.toPng();

                            var diffOutputPath = testOutputPath.resolve(testOutputFileBase + ".diff.png");
                            Files.write(diffOutputPath, diffImagePngData);
                            System.out.println("Error diff image saved to " + diffOutputPath);
                            return 1;
                        }
                    }

                    System.out.println(
                            "Images match. Total Pixels:" + compareResult.totalPixels() + ", Transparent Pixels: " + compareResult.transparentPixels() + ", Percent difference: " + percentDifference + "%");
                    return 0;
                }
            }
        } catch (Exception e) {
            System.err.println("Failed to run test: " + e);
            return 1;
        }
    }

    private static void initializeAlphaSkia() throws IOException {
        switch(getOperatingSystemRid()) {
            case "macos":
                AlphaSkiaMacOs.INSTANCE.initialize();
                break;
            case "win":
                AlphaSkiaWindows.INSTANCE.initialize();
                break;
            case "linux":
                AlphaSkiaLinux.INSTANCE.initialize();
                break;
        }
    }

    private static String getOperatingSystemRid() {
        var os = System.getProperty("os.name");
        if (os.startsWith("Mac")) {
            return "macos";
        }
        if (os.startsWith("Win")) {
            return "win";
        }
        if (os.startsWith("Linux")) {
            return "linux";
        }

        throw new IllegalStateException("Unsupported OS: " + os);
    }

    private static String getAlphaSkiaTestRid() {
        return getOperatingSystemRid();
    }
}