using System.IO.Compression;
using System.Text;
using CitiesService.Application.Common.Helpers;

namespace CitiesService.Tests.Helpers;

public class GzipHelperV2Tests
{
	[Fact]
	public async Task CompressAndDecompress_Roundtrip_File()
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "CitiesService.Tests", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(tempDir);

		try
		{
			var originalPath = Path.Combine(tempDir, "payload.txt");
			var originalContent = "hello world";
			await File.WriteAllTextAsync(originalPath, originalContent, CancellationToken.None);

			var gz = await GzipHelperV2.CompressAsync(
				new FileInfo(originalPath),
				logger: null,
				compressionLevel: CompressionLevel.Optimal,
				overwrite: false,
				cancellationToken: CancellationToken.None);
			Assert.True(gz.Exists);
			Assert.Equal(originalPath + ".gz", gz.FullName);

			var decompressed = await GzipHelperV2.DecompressAsync(
				gz,
				logger: null,
				overwrite: true,
				cancellationToken: CancellationToken.None);
			Assert.True(decompressed.Exists);
			Assert.Equal(originalPath, decompressed.FullName);

			var roundtrip = await File.ReadAllTextAsync(decompressed.FullName, CancellationToken.None);
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
	public async Task CompressAsync_WhenOutputExists_AndOverwriteFalse_Throws()
	{
		var tempDir = Path.Combine(Path.GetTempPath(), "CitiesService.Tests", Guid.NewGuid().ToString("N"));
		Directory.CreateDirectory(tempDir);

		try
		{
			var originalPath = Path.Combine(tempDir, "payload.txt");
			await File.WriteAllTextAsync(originalPath, "content", CancellationToken.None);

			// Create an output file upfront
			await File.WriteAllBytesAsync(originalPath + ".gz", Encoding.UTF8.GetBytes("x"), CancellationToken.None);

			await Assert.ThrowsAsync<IOException>(async () =>
				await GzipHelperV2.CompressAsync(
					new FileInfo(originalPath),
					logger: null,
					overwrite: false,
					cancellationToken: CancellationToken.None));
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
