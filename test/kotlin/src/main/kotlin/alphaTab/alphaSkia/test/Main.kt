package alphaTab.alphaSkia.test

import alphaTab.alphaSkia.AlphaSkiaCanvas
import alphaTab.alphaSkia.AlphaSkiaImage
import alphaTab.alphaSkia.AlphaSkiaLinux
import alphaTab.alphaSkia.AlphaSkiaMacOs
import alphaTab.alphaSkia.AlphaSkiaWindows
import alphaTab.alphaSkia.test.PixelMatch.match
import java.nio.file.Files
import java.nio.file.Path
import java.nio.file.Paths
import kotlin.system.exitProcess

fun main() {
    exitProcess(mainWithExitCode())
}

fun findRepositoryRoot(current: Path): Path {
    if (current.resolve(".nuke").toFile().exists()) {
        return current
    }
    val parent = current.parent
        ?: throw IllegalStateException("Could not find repository root")
    return findRepositoryRoot(parent)
}

fun renderFullImage(): AlphaSkiaImage? {
    AlphaSkiaCanvas().use { fullImageCanvas ->
        println(
            "Begin render with size " + AlphaTabGeneratedRenderTest.totalWidth + "x" + AlphaTabGeneratedRenderTest.totalHeight + " at scale " + AlphaTabGeneratedRenderTestBase.renderScale
        )
        fullImageCanvas.beginRender(
            AlphaTabGeneratedRenderTest.totalWidth,
            AlphaTabGeneratedRenderTest.totalHeight,
            AlphaTabGeneratedRenderTestBase.renderScale
        )
        AlphaSkiaCanvas().use { partialCanvas ->
            val allParts: Array<RenderFunction> =
                AlphaTabGeneratedRenderTest.allParts
            val partPositions: Array<FloatArray> =
                AlphaTabGeneratedRenderTest.partPositions
            for (i in allParts.indices) {
                println("Render part $i")
                allParts[i](partialCanvas).use { part ->
                    val x = partPositions[i][0]
                    val y = partPositions[i][1]
                    val w = partPositions[i][2]
                    val h = partPositions[i][3]
                    println("Drawing part $i into full image at $x/$y/$w/$h")
                    fullImageCanvas.drawImage(part!!, x, y, w, h)
                }
            }
        }
        val fullImage = fullImageCanvas.endRender()
        println("Render of full image completed")
        return fullImage
    }
}

fun mainWithExitCode(): Int {
    try {
        initializeAlphaSkia()
        val tolerancePercent = 1.0
        val repositoryRoot = findRepositoryRoot(Paths.get(".").toAbsolutePath())

        // Load all fonts for rendering
        println("Loading fonts")
        val testDataPath = repositoryRoot.resolve("test/test-data")
        AlphaTabGeneratedRenderTestBase.musicTypeface = (
                AlphaTabGeneratedRenderTestBase.loadTypeface(
                    "Bravura", false, false,
                    testDataPath.resolve("font/bravura/Bravura.ttf")
                )
                )
        AlphaTabGeneratedRenderTestBase.loadTypeface(
            "Roboto", false, false,
            testDataPath.resolve("font/roboto/Roboto-Regular.ttf")
        )
        AlphaTabGeneratedRenderTestBase.loadTypeface(
            "Roboto", true, false,
            testDataPath.resolve("font/roboto/Roboto-Bold.ttf")
        )
        AlphaTabGeneratedRenderTestBase.loadTypeface(
            "Roboto", false, true,
            testDataPath.resolve("font/roboto/Roboto-Italic.ttf")
        )
        AlphaTabGeneratedRenderTestBase.loadTypeface(
            "Roboto", true, true,
            testDataPath.resolve("font/roboto/Roboto-BoldItalic.ttf")
        )
        AlphaTabGeneratedRenderTestBase.loadTypeface(
            "PT Serif", false, false,
            testDataPath.resolve("font/ptserif/PTSerif-Regular.ttf")
        )
        AlphaTabGeneratedRenderTestBase.loadTypeface(
            "PT Serif", true, false,
            testDataPath.resolve("font/ptserif/PTSerif-Bold.ttf")
        )
        AlphaTabGeneratedRenderTestBase.loadTypeface(
            "PT Serif", false, true,
            testDataPath.resolve("font/ptserif/PTSerif-Italic.ttf")
        )
        AlphaTabGeneratedRenderTestBase.loadTypeface(
            "PT Serif", true, true,
            testDataPath.resolve("font/ptserif/PTSerif-BoldItalic.ttf")
        )
        println("Fonts loaded")

        // render full image
        println("Rendering image")
        renderFullImage().use { actualImage ->
            println("Image rendered")

            // save png for future reference
            println("Saving image as PNG")
            val testOutputPath =
                repositoryRoot.resolve("test/test-outputs/java")
            testOutputPath.toFile().mkdirs()
            val testOutputFile =
                testOutputPath.resolve(alphaSkiaTestRid + ".png")
            val pngData = actualImage!!.toPng()
                ?: throw IllegalStateException("Failed to encode final image to png")
            Files.write(testOutputFile, pngData)
            println("Image saved to $testOutputFile")

            // load reference image
            val testReferencePath =
                testDataPath.resolve("reference/" + alphaSkiaTestRid + ".png")
            println("Loading reference image $testReferencePath")
            val testReferenceData =
                Files.readAllBytes(testReferencePath)
            AlphaSkiaImage.decode(testReferenceData).use { expectedImage ->
                checkNotNull(expectedImage) { "Failed to decode reference image" }
                println("Reference image loaded")

                // compare images
                println("Comparing images")
                val actualWidth = actualImage.width
                val actualHeight = actualImage.height
                val expectedWidth = expectedImage.width
                val expectedHeight = expectedImage.height
                if (actualWidth != expectedWidth || actualHeight != expectedHeight) {
                    System.err.println(
                        "Image sizes do not match: Actual[" + actualWidth + "x" + actualHeight + "] Expected[" + expectedWidth + "x" + actualHeight + "]"
                    )
                    return 1
                }
                val actualPixels = actualImage.readPixels()
                val expectedPixels = expectedImage.readPixels()
                val diffPixels = ByteArray(actualPixels!!.size)
                val compareResult = match(
                    actualPixels,
                    expectedPixels!!,
                    diffPixels,
                    actualWidth,
                    actualHeight,
                    PixelMatchOptions(
                        0.3,
                        PixelMatchColor(255.toByte(), 0.toByte(), 0.toByte())
                    )
                )
                val totalPixels =
                    compareResult.totalPixels - compareResult.transparentPixels
                val percentDifference: Double =
                    compareResult.differentPixels.toDouble() / totalPixels * 100
                val pass = percentDifference < tolerancePercent
                if (!pass) {
                    System.err.println(
                        "Difference between original and new image is too big: " + compareResult.differentPixels + "/" + totalPixels + "(" + percentDifference + "%)"
                    )
                    AlphaSkiaImage.fromPixels(actualWidth, actualHeight, diffPixels)
                        .use { diffImage ->
                            val diffImagePngData = diffImage!!.toPng()!!
                            val diffOutputPath =
                                testOutputPath.resolve(alphaSkiaTestRid + ".diff.png")
                            Files.write(diffOutputPath, diffImagePngData)
                            println("Error diff image saved to $diffOutputPath")
                            return 1
                        }
                }
                System.out.println(
                    "Images match. Total Pixels:" + compareResult.totalPixels + ", Transparent Pixels: " + compareResult.transparentPixels + ", Percent difference: " + percentDifference + "%"
                )
                return 0
            }
        }
    } catch (e: Exception) {
        System.err.println("Failed to run test: $e")
        return 1
    }
}

fun initializeAlphaSkia() {
    when (operatingSystemRid) {
        "macos" -> AlphaSkiaMacOs.INSTANCE.initialize()
        "win" -> AlphaSkiaWindows.INSTANCE.initialize()
        "linux" -> AlphaSkiaLinux.INSTANCE.initialize()
    }
}

val operatingSystemRid: String
    get() {
        val os = System.getProperty("os.name")
        if (os.startsWith("Mac")) {
            return "macos"
        }
        if (os.startsWith("Win")) {
            return "win"
        }
        if (os.startsWith("Linux")) {
            return "linux"
        }
        throw IllegalStateException("Unsupported OS: $os")
    }

val alphaSkiaTestRid: String
    get() = operatingSystemRid
