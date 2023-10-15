using System.Runtime.InteropServices;

namespace AlphaSkia.Test;

public class Program
{
    private static string FindRepositoryRoot(string current)
    {
        if (Directory.Exists(Path.Combine(current, ".nuke")))
        {
            return current;
        }

        var parent = Path.GetDirectoryName(current);
        if (parent == null)
        {
            throw new InvalidOperationException("Could not find repository root");
        }

        return FindRepositoryRoot(parent);
    }

    private static AlphaSkiaImage RenderFullImage()
    {
        using var fullImageCanvas = new AlphaSkiaCanvas();
        Console.WriteLine(
            $"Begin render with size {AlphaTabGeneratedRenderTest.TotalWidth}x{AlphaTabGeneratedRenderTest.TotalHeight} at scale {AlphaTabGeneratedRenderTest.RenderScale}");
        fullImageCanvas.BeginRender(AlphaTabGeneratedRenderTest.TotalWidth, AlphaTabGeneratedRenderTest.TotalHeight,
            AlphaTabGeneratedRenderTest.RenderScale);

        using var partialCanvas = new AlphaSkiaCanvas();
        for (var i = 0; i < AlphaTabGeneratedRenderTest.AllParts.Length; i++)
        {
            Console.WriteLine($"Render part {i}");
            using var part = AlphaTabGeneratedRenderTest.AllParts[i](partialCanvas);

            var x = AlphaTabGeneratedRenderTest.PartPositions[i, 0];
            var y = AlphaTabGeneratedRenderTest.PartPositions[i, 1];
            var w = AlphaTabGeneratedRenderTest.PartPositions[i, 2];
            var h = AlphaTabGeneratedRenderTest.PartPositions[i, 3];

            Console.WriteLine($"Drawing part {i} into full image at {x}/{y}/{w}/{h}");
            fullImageCanvas.DrawImage(part, x, y, w, h);
        }

        var fullImage = fullImageCanvas.EndRender();
        Console.WriteLine("Render of full image completed");
        return fullImage!;
    }

    public static int Main()
    {
        const double tolerancePercent = 1;
        try
        {
            var repositoryRoot = FindRepositoryRoot(Path.GetFullPath("."));

            // Load all fonts for rendering
            Console.WriteLine("Loading fonts");
            var testDataPath = Path.Combine(repositoryRoot, "test", "test-data");
            AlphaTabGeneratedRenderTest.MusicTypeface = AlphaTabGeneratedRenderTest.LoadTypeface("Bravura", false, false,
                Path.Combine(testDataPath, "font", "bravura", "Bravura.ttf"));
            AlphaTabGeneratedRenderTest.LoadTypeface("Roboto", false, false,
                Path.Combine(testDataPath, "font", "roboto", "Roboto-Regular.ttf"));
            AlphaTabGeneratedRenderTest.LoadTypeface("Roboto", true, false,
                Path.Combine(testDataPath, "font", "roboto", "Roboto-Bold.ttf"));
            AlphaTabGeneratedRenderTest.LoadTypeface("Roboto", false, true,
                Path.Combine(testDataPath, "font", "roboto", "Roboto-Italic.ttf"));
            AlphaTabGeneratedRenderTest.LoadTypeface("Roboto", true, true,
                Path.Combine(testDataPath, "font", "roboto", "Roboto-BoldItalic.ttf"));
            AlphaTabGeneratedRenderTest.LoadTypeface("PT Serif", false, false,
                Path.Combine(testDataPath, "font", "ptserif", "PTSerif-Regular.ttf"));
            AlphaTabGeneratedRenderTest.LoadTypeface("PT Serif", true, false,
                Path.Combine(testDataPath, "font", "ptserif", "PTSerif-Bold.ttf"));
            AlphaTabGeneratedRenderTest.LoadTypeface("PT Serif", false, true,
                Path.Combine(testDataPath, "font", "ptserif", "PTSerif-Italic.ttf"));
            AlphaTabGeneratedRenderTest.LoadTypeface("PT Serif", true, true,
                Path.Combine(testDataPath, "font", "ptserif", "PTSerif-BoldItalic.ttf"));
            Console.WriteLine("Fonts loaded");

            // render full image
            Console.WriteLine("Rendering image");
            using var actualImage = RenderFullImage();
            Console.WriteLine("Image rendered");

            // save png for future reference
            Console.WriteLine("Saving image as PNG");

            var testOutputPath = Path.Combine(repositoryRoot, "test", "test-outputs", "dotnet");
            Directory.CreateDirectory(testOutputPath);

            var testOutputFile = Path.Combine(testOutputPath, AlphaSkiaTestRid + ".png");
            var pngData = actualImage.ToPng();
            if (pngData == null)
            {
                throw new InvalidOperationException("Failed to encode final image to png");
            }

            File.WriteAllBytes(testOutputFile, pngData);
            Console.WriteLine($"Image saved to {testOutputFile}");

            // load reference image
            var testReferencePath = Path.Combine(testDataPath, "reference", AlphaSkiaTestRid + ".png");
            Console.WriteLine($"Loading reference image {testReferencePath}");

            var testReferenceData = File.ReadAllBytes(testReferencePath);
            using var expectedImage = AlphaSkiaImage.Decode(testReferenceData);
            if (expectedImage == null)
            {
                throw new InvalidOperationException("Failed to decode reference image");
            }

            Console.WriteLine("Reference image loaded");

            // compare images
            Console.WriteLine("Comparing images");
            var actualWidth = actualImage.Width;
            var actualHeight = actualImage.Height;

            var expectedWidth = expectedImage.Width;
            var expectedHeight = expectedImage.Height;
            if (actualWidth != expectedWidth || actualHeight != expectedHeight)
            {
                Console.Error.WriteLine(
                    $"Image sizes do not match: Actual[{actualWidth}x{actualHeight}] Expected[{expectedWidth}x{actualHeight}]");
                return 1;
            }

            var actualPixels = actualImage.ReadPixels()!;
            var expectedPixels = expectedImage.ReadPixels()!;
            var diffPixels = new byte[actualPixels.Length];

            var compareResult = PixelMatch.Match(actualPixels,
                expectedPixels,
                diffPixels,
                actualWidth,
                actualHeight, new PixelMatchOptions
                {
                    Threshold = 0.3,
                    DiffColor = new PixelMatchColor(255, 0, 0)
                });

            var totalPixels = compareResult.TotalPixels - compareResult.TransparentPixels;
            var percentDifference = ((double)compareResult.DifferentPixels / totalPixels) * 100;
            var pass = percentDifference < tolerancePercent;
            if (!pass)
            {
                Console.Error.WriteLine(
                    $"Difference between original and new image is too big: {compareResult.DifferentPixels}/{totalPixels}({percentDifference}%)");

                // create diff image as PNG
                using var diffImage = AlphaSkiaImage.FromPixels(actualWidth, actualHeight, diffPixels)!;
                var diffImagePngData = diffImage.ToPng()!;

                var diffOutputPath = Path.Combine(testOutputPath, AlphaSkiaTestRid + ".diff.png");
                File.WriteAllBytes(diffOutputPath, diffImagePngData);
                Console.WriteLine($"Error diff image saved to {diffOutputPath}");
                return 1;
            }

            Console.WriteLine(
                $"Images match. Total Pixels:{compareResult.TotalPixels}, Transparent Pixels: {compareResult.TransparentPixels}, Percent difference: {percentDifference}%");
            return 0;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to run test: {e}");
            return 1;
        }
    }

    private static readonly string AlphaSkiaTestRid = OperatingSystem.IsWindows()
        ? "win"
        : OperatingSystem.IsLinux()
            ? "linux"
            : OperatingSystem.IsMacOS()
                ? "maxos"
                : OperatingSystem.IsAndroid()
                    ? "android"
                    : OperatingSystem.IsIOS()
                        ? "ios"
                        : throw new PlatformNotSupportedException("Unsupported test platform");
}