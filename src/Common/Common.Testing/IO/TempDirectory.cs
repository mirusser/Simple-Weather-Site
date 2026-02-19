
namespace Common.Testing.IO;

/// <summary>
/// Creates and cleans up a unique temporary directory.
///
/// Why: tests often need file-system IO (gzip, download fixtures, etc.).
/// This helper centralizes the "create unique temp dir" + "best-effort cleanup" pattern.
/// </summary>
public sealed class TempDirectory : IDisposable
{
    public string Path { get; }

    private TempDirectory(string path) => Path = path;

    public static TempDirectory Create(string prefix = "tests")
    {
        var dir = System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            prefix,
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(dir);
        return new TempDirectory(dir);
    }

    public string Combine(params string[] parts)
    {
        var all = new string[parts.Length + 1];
        all[0] = Path;
        Array.Copy(parts, 0, all, 1, parts.Length);
        return System.IO.Path.Combine(all);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
        catch
        {
            // best-effort cleanup; do not fail the test due to temp dir deletion issues.
        }
    }
}
