namespace TestGenerator;

public class PathUtils
{
    private static string? _repositoryRoot;

    public static string RepositoryRoot
    {
        get
        {
            return _repositoryRoot ??= FindRootDirectory(Path.GetFullPath(Environment.CurrentDirectory)) ??
                                       FindRootDirectory(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory)) ??
                                       throw new InvalidOperationException(
                                           "Need to execute generator in a working directory of the test");
        }
    }
    
    private static string? FindRootDirectory(string current)
    {
        if (Directory.Exists(Path.Combine(current, ".nuke")))
        {
            return current;
        }

        var parent = Path.GetDirectoryName(current);
        if (parent == null)
        {
            return null;
        }

        return FindRootDirectory(parent);
    }

}