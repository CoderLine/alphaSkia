using System.Globalization;
using System.Reflection;
using TestGenerator;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

// TODO: make public again
// typeof(AlphaTab.Environment).GetProperty("HighDpiFactor", BindingFlags.Static | BindingFlags.NonPublic)!.SetValue(null, 1.5);

var rootDir = PathUtils.RepositoryRoot;

var nativeTestFileCpp = Path.Combine(rootDir, "test", "native", "src", "AlphaTabGeneratedTest.cpp");
var nativeTestFileH = Path.Combine(rootDir, "test", "native", "include", "AlphaTabGeneratedTest.h");
Directory.CreateDirectory(Path.GetDirectoryName(nativeTestFileCpp)!);
File.WriteAllText(nativeTestFileCpp, AlphaSkiaUnitTestGenerator.GenerateCppSource(out var headerSource));
Console.WriteLine("Updated C++ source in {0}", nativeTestFileCpp);
Directory.CreateDirectory(Path.GetDirectoryName(nativeTestFileH)!);
File.WriteAllText(nativeTestFileH, headerSource);
Console.WriteLine("Updated C++ Header in {0}", nativeTestFileH);

var csharpTestFile = Path.Combine(rootDir, "test", "dotnet", "AlphaSkia.Test", "AlphaTabGeneratedRenderTest.generated.cs");
Directory.CreateDirectory(Path.GetDirectoryName(csharpTestFile)!);
File.WriteAllText(csharpTestFile, AlphaSkiaUnitTestGenerator.GenerateCSharp());
Console.WriteLine("Updated C# source in {0}", csharpTestFile);

var javaTestFile = Path.Combine(rootDir, "test", "java", "src", "main", "generated", "alphaTab", "alphaSkia", "test", 
    "AlphaTabGeneratedRenderTest.java");
Directory.CreateDirectory(Path.GetDirectoryName(javaTestFile)!);
File.WriteAllText(javaTestFile, AlphaSkiaUnitTestGenerator.GenerateJava());
Console.WriteLine("Updated Java source in {0}", javaTestFile);

var typeScriptTestFile = Path.Combine(rootDir, "test", "node", "AlphaTabGeneratedRenderTest.ts");
Directory.CreateDirectory(Path.GetDirectoryName(typeScriptTestFile)!);
File.WriteAllText(typeScriptTestFile, AlphaSkiaUnitTestGenerator.GenerateTypeScript());
Console.WriteLine("Updated TypeScript source in {0}", javaTestFile);