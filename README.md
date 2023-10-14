# alphaSkia

> ℹ️ This library is still work in progress and not yet available. 

alphaSkia is a special cross platform [Skia](https://skia.org/) wrapper used in [alphaTab](https://github.com/CoderLine/alphaTab) it aims to provide a HTML5 Canvas like API to alphaTab providing a consistent rendering experience across the alphaTab flavours like web, .net and Kotlin. Differences across operating systems might still apply.

## Credits
A big _"Thank you"_ to the following projects giving me good insights on how to build Skia in a cross platform fashion. These projects influenced heavily how the pipelines of alphaSkia are organized.

* [SkiaSharp](https://github.com/mono/SkiaSharp/)
* [Skija](https://github.com/HumbleUI/Skija/)

## Supported platforms

In the initial release of the library we plan to support following platform matrix:

| Target              | .net | Java | Node.js |
|---------------------|------|------|---------|
| win-x64             | ✅    | ✅    | ✅       |
| win-x86             | ✅    | ✅    | ✅       |
| win-arm64           | ✅    | ✅    | ✅       |
| linux-x64           | ✅    | ✅    | ✅       |
| linux-x86           | ✅    | ✅    | ✅       |
| linux-arm           | ✅    | ✅    | ✅       |
| linux-arm64         | ✅    | ✅    | ✅       |
| macos-x64           | ✅    | ✅    | ✅       |
| macos-arm64         | ✅    | ✅    | ✅       |
| android-x64         | ✅    | ✅    |         |
| android-x86         | ✅    | ✅    |         |
| android-arm         | ✅    | ✅    |         |
| android-arm64       | ✅    | ✅    |         |
| ios-arm64           | ✅    |      |         |
| iossimulator-x64    | ✅    |      |         |
| iossimulator-arm64  | ✅    |      |         |
