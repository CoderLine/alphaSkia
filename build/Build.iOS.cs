using System.Collections.Generic;

partial class Build
{
    void BuildLibAlphaSkiaiOS()
    {
        var gnArgs = new Dictionary<string, string>();

        if (TargetOs == TargetOperatingSystem.iOSSimulator)
        {
            gnArgs["ios_use_simulator"] = "true";
        }
        
        string[] filesToCopy;
        var isShared = Variant == Variant.Shared;
        if (isShared)
        {
            filesToCopy = new[]
            {
                "libAlphaSkia.dylib"
            };
        }
        else
        {
            filesToCopy = new[]
            {
                "libAlphaSkia.a",
                "libskia.a"
            };
        }

        BuildSkiaiOS("libAlphaSkia", gnArgs, filesToCopy);
    }
    
    void BuildSkiaiOS(string buildTarget, Dictionary<string, string> gnArgs,
        string[] filesToCopy)
    {
        gnArgs["skia_use_system_freetype2"] = "false";
        gnArgs["skia_use_metal"] = "true";

        BuildSkia(buildTarget, gnArgs, filesToCopy);
    }
}