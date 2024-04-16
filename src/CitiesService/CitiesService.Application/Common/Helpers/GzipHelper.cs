using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CitiesService.Application.Common.Helpers;

public static class GzipHelper
{
	public static async Task CompressAsync(
		FileInfo fileToCompress,
		ILogger? logger = null,
		CancellationToken cancellationToken = default)
	{
		await using FileStream originalFileStream = fileToCompress.OpenRead();

		if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden) == FileAttributes.Hidden
			|| fileToCompress.Extension == ".gz")
		{
			return;
		}

		await using FileStream compressedFileStream = File.Create(fileToCompress.FullName + ".gz");
		await using GZipStream compressionStream = new(compressedFileStream, CompressionMode.Compress);
		await originalFileStream.CopyToAsync(compressionStream, cancellationToken);

		logger?.LogInformation(
			"Compressed {FileName} from {FileLength} to {CompressedFileLength} bytes.",
			fileToCompress.Name,
			fileToCompress.Length.ToString(),
			compressedFileStream.Length.ToString());
	}

	public static async Task DecompressAsync(
		FileInfo fileToDecompress,
		ILogger? logger = null,
		CancellationToken cancellationToken = default)
	{
		await using FileStream originalFileStream = fileToDecompress.OpenRead();
		string currentFileName = fileToDecompress.FullName;
		string newFileName = currentFileName.Remove(currentFileName.Length - fileToDecompress.Extension.Length);

		await using FileStream decompressedFileStream = File.Create(newFileName);
		await using GZipStream decompressionStream = new(originalFileStream, CompressionMode.Decompress);
		await decompressionStream.CopyToAsync(decompressedFileStream, cancellationToken);

		logger?.LogInformation(
			"Decompressed: {FileName}",
			fileToDecompress.Name);
	}
}