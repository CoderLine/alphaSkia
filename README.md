# alphaSkia

alphaSkia is a special cross platform [Skia](https://skia.org/) wrapper used in [alphaTab](https://github.com/CoderLine/alphaTab) it aims to provide a HTML5 Canvas like API to alphaTab providing a consistent rendering experience across the alphaTab flavours like web, .net and Kotlin. Differences across operating systems might still apply.

## Versioning 

alphaSkia a slightly adapted [Semantic Versioning](https://semver.org/) scheme where the `PATCH` part indicates
the Skia milestone version we have integrated. 

Currently we are on [Skia m135](https://github.com/CoderLine/alphaSkia/blob/main/.gitmodules#L8)

Given a version number `MAJOR.MINOR.SKIA`:

1. `MAJOR` is incremented when we make incompatible API changes. 
2. `MINOR` is incremented when we add functionality in a backward compatible manner
3. `SKIA` is aligned with the Skia milestone we have integrated. See [here](https://skia.org/docs/user/release/#release-process) and [here](https://chromiumdash.appspot.com/schedule)

The prerelease labels and tags we use: 

* Pre-Release versions for builds which are in-development
    * SemVer: `-alpha.<build counter>` (NuGet and NPM)
    * NPM Tag: `alpha`
    * Maven: `-SNAPSHOT`
* Local Builds (not published anywhere)
    * SemVer: `-local`
    * Maven: `-LOCAL`

## Supported platforms

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

We don't have a full automatic test matrix for all packages and platforms. If you encounter any platform problems please report them in this repository.

## Credits
A big **Thank you** to the following projects giving me good insights on how to build Skia in a cross platform fashion. These projects influenced heavily how the pipelines of alphaSkia are organized.

* [SkiaSharp](https://github.com/mono/SkiaSharp/)
* [Skija](https://github.com/HumbleUI/Skija/)

