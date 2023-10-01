using TestGenerator;

var rootDir = PathUtils.RepositoryRoot;

var csharpTestFile = Path.Combine(rootDir, "lib", "dotnet", "AlphaSkia.Test", "AlphaTabGeneratedTest.cs");
File.WriteAllText(csharpTestFile, AlphaSkiaUnitTestGenerator.GenerateCSharp());
Console.WriteLine("Updated C# source in {0}", csharpTestFile);

var javaTestFile = Path.Combine(rootDir, "lib", "java", "main", "src", "test", "java", "alphaskia",
    "AlphaTabGeneratedRenderTest.java");
File.WriteAllText(javaTestFile, AlphaSkiaUnitTestGenerator.GenerateJava());
Console.WriteLine("Updated Java source in {0}", javaTestFile);