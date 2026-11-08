using System;
using System.IO;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace PatientHealthRecordsEPJ.ImageConversion;

public static class ImageBase64Converter
{
	public static string TiffBase64ToPngBase64(
		string tiffBase64,
		int pageIndex = 0,
		PngCompressionLevel pngCompressionLevel = PngCompressionLevel.Level6)
	{
		if (string.IsNullOrWhiteSpace(tiffBase64))
			throw new ArgumentException("Input is empty.", nameof(tiffBase64));

		byte[] tiffBytes = Convert.FromBase64String(tiffBase64);
		using var inputStream = new MemoryStream(tiffBytes, writable: false);

		// Auto-detects and loads TIFF
		using Image<Rgba32> image = Image.Load<Rgba32>(inputStream);

		if (image.Frames.Count == 0)
			throw new InvalidDataException("TIFF has no frames.");

		pageIndex = Math.Clamp(pageIndex, 0, image.Frames.Count - 1);

		// Clone the desired page as its own single-frame Image<Rgba32>
		using Image<Rgba32> singleFrameImage = image.Frames.CloneFrame(pageIndex);

		var pngEncoder = new PngEncoder
		{
			CompressionLevel = pngCompressionLevel,
			ColorType = PngColorType.RgbWithAlpha
		};

		using var outputStream = new MemoryStream();
		singleFrameImage.Save(outputStream, pngEncoder);
		return Convert.ToBase64String(outputStream.ToArray());
	}

	public static List<string> TiffBase64ToAllPngPages(string tiffBase64, PngCompressionLevel level = PngCompressionLevel.Level6)
	{
		var results = new List<string>();
		byte[] tiffBytes = Convert.FromBase64String(tiffBase64);
		using var inputStream = new MemoryStream(tiffBytes, writable: false);
		using Image<Rgba32> image = Image.Load<Rgba32>(inputStream);

		var encoder = new PngEncoder { CompressionLevel = level, ColorType = PngColorType.RgbWithAlpha };

		for (int i = 0; i < image.Frames.Count; i++)
		{
			using Image<Rgba32> page = image.Frames.CloneFrame(i);
			using var ms = new MemoryStream();
			page.Save(ms, encoder);
			results.Add(Convert.ToBase64String(ms.ToArray()));
		}

		return results;
	}
}
