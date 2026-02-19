using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace CitiesService.Application.Common.Helpers;

public static class GzipHelperV2
{
	private const string FileExtension = ".gz";

	public static async Task CompressAsync(
		Stream source,
		Stream destination,
		CompressionLevel compressionLevel = CompressionLevel.Optimal,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(destination);

		await using var compressionStream = new GZipStream(
			destination,
			compressionLevel,
			leaveOpen: true);

		await source.CopyToAsync(compressionStream, cancellationToken);
	}

	public static async Task DecompressAsync(
		Stream source,
		Stream destination,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(source);
		ArgumentNullException.ThrowIfNull(destination);

		await using var decompressionStream = new GZipStream(
			source,
			CompressionMode.Decompress,
			leaveOpen: true);

		await decompressionStream.CopyToAsync(destination, cancellationToken);
	}

	public static async Task<FileInfo> CompressAsync(
		FileInfo fileToCompress,
		ILogger? logger = null,
		CompressionLevel compressionLevel = CompressionLevel.Optimal,
		bool overwrite = false,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(fileToCompress);

		if (!fileToCompress.Exists)
		{
			throw new FileNotFoundException("File to compress was not found.", fileToCompress.FullName);
		}

		if (fileToCompress.Extension.Equals(FileExtension, StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentException("File already has a .gz extension.", nameof(fileToCompress));
		}

		var attributes = File.GetAttributes(fileToCompress.FullName);
		if ((attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
		{
			throw new InvalidOperationException("Refusing to compress a hidden file.");
		}

		var outputPath = fileToCompress.FullName + FileExtension;
		var outputFile = new FileInfo(outputPath);

		if (outputFile.Exists && !overwrite)
		{
			throw new IOException($"Output file already exists: '{outputPath}'.");
		}

		await using (var originalFileStream = fileToCompress.OpenRead())
		await using (var compressedFileStream = new FileStream(
			outputPath,
			overwrite ? FileMode.Create : FileMode.CreateNew,
			FileAccess.Write,
			FileShare.None))
		{
			await CompressAsync(originalFileStream, compressedFileStream, compressionLevel, cancellationToken);
		}

		// Re-stat after streams are disposed (final sizes)
		outputFile.Refresh();
		fileToCompress.Refresh();

		logger?.LogInformation(
			"Compressed {FileName} from {FileLength} to {CompressedFileLength} bytes.",
			fileToCompress.Name,
			fileToCompress.Length,
			outputFile.Length);

		return outputFile;
	}

	public static async Task<FileInfo> DecompressAsync(
		FileInfo fileToDecompress,
		ILogger? logger = null,
		bool overwrite = false,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(fileToDecompress);

		if (!fileToDecompress.Exists)
		{
			throw new FileNotFoundException("File to decompress was not found.", fileToDecompress.FullName);
		}

		if (!fileToDecompress.Extension.Equals(FileExtension, StringComparison.OrdinalIgnoreCase))
		{
			throw new ArgumentException("File does not have a .gz extension.", nameof(fileToDecompress));
		}

		var outputPath = Path.Combine(
			fileToDecompress.DirectoryName ?? string.Empty,
			Path.GetFileNameWithoutExtension(fileToDecompress.Name));
		var outputFile = new FileInfo(outputPath);

		if (outputFile.Exists && !overwrite)
		{
			throw new IOException($"Output file already exists: '{outputPath}'.");
		}

		await using (var originalFileStream = fileToDecompress.OpenRead())
		await using (var decompressedFileStream = new FileStream(
			outputPath,
			overwrite ? FileMode.Create : FileMode.CreateNew,
			FileAccess.Write,
			FileShare.None))
		{
			await DecompressAsync(originalFileStream, decompressedFileStream, cancellationToken);
		}

		logger?.LogInformation(
			"Decompressed: {FileName}",
			fileToDecompress.Name);

		return outputFile;
	}
}
