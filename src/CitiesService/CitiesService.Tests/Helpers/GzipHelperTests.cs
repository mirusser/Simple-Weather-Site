using System.IO.Compression;
using System.Text;
using CitiesService.Application.Common.Helpers;

namespace CitiesService.Tests.Helpers;

public class GzipHelperTests
{
    [Fact]
    public async Task DecompressAsync_WritesDecompressedFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "CitiesService.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var originalContent = "hello world";
            var gzPath = Path.Combine(tempDir, "payload.txt.gz");
            var decompressedPath = Path.Combine(tempDir, "payload.txt");

            await using (var fs = new FileStream(gzPath, FileMode.Create, FileAccess.Write, FileShare.None))
            await using (var gzip = new GZipStream(fs, CompressionMode.Compress))
            {
                var bytes = Encoding.UTF8.GetBytes(originalContent);
                await gzip.WriteAsync(bytes, CancellationToken.None);
            }

            await GzipHelper.DecompressAsync(new FileInfo(gzPath), logger: null, cancellationToken: CancellationToken.None);

            Assert.True(File.Exists(decompressedPath));
            var roundtrip = await File.ReadAllTextAsync(decompressedPath, CancellationToken.None);
            Assert.Equal(originalContent, roundtrip);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public async Task CompressAsync_WritesGzipFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "CitiesService.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            var path = Path.Combine(tempDir, "payload.txt");
            await File.WriteAllTextAsync(path, "content", CancellationToken.None);

            await GzipHelper.CompressAsync(new FileInfo(path), logger: null, cancellationToken: CancellationToken.None);

            Assert.True(File.Exists(path + ".gz"));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
