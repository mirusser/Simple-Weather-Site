using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CitiesService.Application.Common.Helpers;

/// <summary>
/// Provides methods for compressing and decompressing files using the GZip compression algorithm.
/// </summary>
public static class GzipHelper
{
	private const string fileExtension = ".gz";

	/// <summary>
	/// Compresses a specified file using GZip compression if the file is not hidden and does not already have a .gz extension.
	/// </summary>
	public static async Task CompressAsync(
		FileInfo fileToCompress,
		ILogger? logger = null,
		CancellationToken cancellationToken = default)
	{
		await using FileStream originalFileStream = fileToCompress.OpenRead();

		if ((File.GetAttributes(fileToCompress.FullName) & FileAttributes.Hidden) == FileAttributes.Hidden
			|| fileToCompress.Extension == fileExtension)
		{
			return;
		}

		await using FileStream compressedFileStream = File.Create($"fileToCompress.FullName{fileExtension}");
		await using GZipStream compressionStream = new(compressedFileStream, CompressionMode.Compress);
		await originalFileStream.CopyToAsync(compressionStream, cancellationToken);

		logger?.LogInformation(
			"Compressed {FileName} from {FileLength} to {CompressedFileLength} bytes.",
			fileToCompress.Name,
			fileToCompress.Length.ToString(),
			compressedFileStream.Length.ToString());
	}

	/// <summary>
	/// Decompresses a specified GZip-compressed file back to its original format.
	/// </summary>
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