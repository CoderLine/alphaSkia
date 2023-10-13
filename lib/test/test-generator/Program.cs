using System.Globalization;
using TestGenerator;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

var rootDir = PathUtils.RepositoryRoot;

var csharpTestFile = Path.Combine(rootDir, "lib", "dotnet", "AlphaSkia.Test", "AlphaTabGeneratedTest.cs");
File.WriteAllText(csharpTestFile, AlphaSkiaUnitTestGenerator.GenerateCSharp());
Console.WriteLine("Updated C# source in {0}", csharpTestFile);

var javaTestFile = Path.Combine(rootDir, "lib", "java", "main", "src", "test", "java", "net", "alphatab", "alphaskia",
    "AlphaTabGeneratedRenderTest.java");
File.WriteAllText(javaTestFile, AlphaSkiaUnitTestGenerator.GenerateJava());
Console.WriteLine("Updated Java source in {0}", javaTestFile);

var typeScriptTestFile = Path.Combine(rootDir, "lib", "node", "test", "AlphaTabGeneratedRenderTest.test.ts");
File.WriteAllText(typeScriptTestFile, AlphaSkiaUnitTestGenerator.GenerateTypeScript());
Console.WriteLine("Updated TypeScript source in {0}", typeScriptTestFile);