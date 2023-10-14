using System.Globalization;
using TestGenerator;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var rootDir = PathUtils.RepositoryRoot;

var nativeTestFileCpp = Path.Combine(rootDir, "test", "native", "src", "AlphaTabGeneratedTest.cpp");
var nativeTestFileH = Path.Combine(rootDir, "test", "native", "include", "AlphaTabGeneratedTest.h");
File.WriteAllText(nativeTestFileCpp, AlphaSkiaUnitTestGenerator.GenerateCppSource(out var headerSource));
Console.WriteLine("Updated C++ source in {0}", nativeTestFileCpp);
File.WriteAllText(nativeTestFileH, headerSource);
Console.WriteLine("Updated C++ Header in {0}", nativeTestFileH);

// var csharpTestFile = Path.Combine(rootDir, "test", "dotnet", "AlphaSkia.Test", "AlphaTabGeneratedTest.cs");
// File.WriteAllText(csharpTestFile, AlphaSkiaUnitTestGenerator.GenerateCSharp());
// Console.WriteLine("Updated C# source in {0}", csharpTestFile);
//
// var javaTestFile = Path.Combine(rootDir, "test", "src", "main", "java", "net", "alphatab", "alphaskia",
//     "AlphaTabGeneratedRenderTest.java");
// File.WriteAllText(javaTestFile, AlphaSkiaUnitTestGenerator.GenerateJava());
// Console.WriteLine("Updated Java source in {0}", javaTestFile);
//
// var typeScriptTestFile = Path.Combine(rootDir, "test", "node", "src", "AlphaTabGeneratedRenderTest.ts");
// File.WriteAllText(typeScriptTestFile, AlphaSkiaUnitTestGenerator.GenerateTypeScript());
// Console.WriteLine("Updated TypeScript source in {0}", typeScriptTestFile);