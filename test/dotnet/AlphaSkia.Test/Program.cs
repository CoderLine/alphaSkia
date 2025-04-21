namespace AlphaSkia.Test;

public static class Program
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
            fullImageCanvas.DrawImage(part!, x, y, w, h);
        }

        var fullImage = fullImageCanvas.EndRender();
        Console.WriteLine("Render of full image completed");
        return fullImage!;
    }

    public static int Main(string[] args)
    {
        var isFreeType = args.Contains("--freetype");
        if(isFreeType) 
        {
            Console.WriteLine("Switching to FreeType Fonts");
            AlphaSkiaCanvas.SwitchToFreeTypeFonts();
        }

        const double tolerancePercent = 1;
        try
        {
            var repositoryRoot = FindRepositoryRoot(Path.GetFullPath("."));

            // Load all fonts for rendering
            Console.WriteLine("Loading fonts");
            var testDataPath = Path.Combine(repositoryRoot, "test", "test-data");
            var musicTypeface = AlphaTabGeneratedRenderTest.LoadTypeface(Path.Combine(testDataPath, "font", "bravura", "Bravura.otf"));

            AlphaTabGeneratedRenderTest.MusicTextStyle = new AlphaSkiaTextStyle(
                new[] { musicTypeface.FamilyName },
                musicTypeface.Weight,
                musicTypeface.IsItalic
            );
            
            AlphaTabGeneratedRenderTest.LoadTypeface(Path.Combine(testDataPath, "font", "noto-sans", "NotoSans-Regular.otf"));
            AlphaTabGeneratedRenderTest.LoadTypeface(Path.Combine(testDataPath, "font", "noto-sans", "NotoSans-Bold.otf"));
            AlphaTabGeneratedRenderTest.LoadTypeface(Path.Combine(testDataPath, "font", "noto-sans", "NotoSans-Italic.otf"));
            AlphaTabGeneratedRenderTest.LoadTypeface(Path.Combine(testDataPath, "font", "noto-sans", "NotoSans-BoldItalic.otf"));
            AlphaTabGeneratedRenderTest.LoadTypeface(Path.Combine(testDataPath, "font", "noto-serif", "NotoSerif-Regular.otf"));
            AlphaTabGeneratedRenderTest.LoadTypeface(Path.Combine(testDataPath, "font", "noto-serif", "NotoSerif-Bold.otf"));
            AlphaTabGeneratedRenderTest.LoadTypeface(Path.Combine(testDataPath, "font", "noto-serif", "NotoSerif-Italic.otf"));
            AlphaTabGeneratedRenderTest.LoadTypeface(Path.Combine(testDataPath, "font", "noto-serif", "NotoSerif-BoldItalic.otf"));
            AlphaTabGeneratedRenderTest.LoadTypeface(Path.Combine(testDataPath, "font", "noto-music", "NotoMusic-Regular.otf"));
            AlphaTabGeneratedRenderTest.LoadTypeface(Path.Combine(testDataPath, "font", "noto-color-emoji", "NotoColorEmoji_WindowsCompatible.ttf"));
            
            Console.WriteLine("Fonts loaded");

            // render full image
            Console.WriteLine("Rendering image");
            using var actualImage = RenderFullImage();
            Console.WriteLine("Image rendered");

            // save png for future reference
            Console.WriteLine("Saving image as PNG");

            var testOutputPath = Path.Combine(repositoryRoot, "test", "test-outputs", "dotnet");
            Directory.CreateDirectory(testOutputPath);


            var testOutputFileBase = isFreeType ? "freetype" : AlphaSkiaTestRid;

            var testOutputFile = Path.Combine(testOutputPath, testOutputFileBase + ".png");
            var pngData = actualImage.ToPng();
            if (pngData == null)
            {
                throw new InvalidOperationException("Failed to encode final image to png");
            }

            File.WriteAllBytes(testOutputFile, pngData);
            Console.WriteLine($"Image saved to {testOutputFile}");

            // load reference image
            var testReferencePath = Path.Combine(testDataPath, "reference", testOutputFileBase + ".png");
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

                var diffOutputPath = Path.Combine(testOutputPath, testOutputFileBase + ".diff.png");
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

#if NET48
    private static readonly string AlphaSkiaTestRid = Environment.OSVersion.Platform == PlatformID.Win32NT
        ? "win"
        : Environment.OSVersion.Platform == PlatformID.Unix
            ? "linux"
            : Environment.OSVersion.Platform == PlatformID.MacOSX
                ? "macos"
                : throw new PlatformNotSupportedException("Unsupported test platform");
#else
    private static readonly string AlphaSkiaTestRid = OperatingSystem.IsWindows()
        ? "win"
        : OperatingSystem.IsLinux()
            ? "linux"
            : OperatingSystem.IsMacOS()
                ? "macos"
                : OperatingSystem.IsAndroid()
                    ? "android"
                    : OperatingSystem.IsIOS()
                        ? "ios"
                        : throw new PlatformNotSupportedException("Unsupported test platform");
#endif
}