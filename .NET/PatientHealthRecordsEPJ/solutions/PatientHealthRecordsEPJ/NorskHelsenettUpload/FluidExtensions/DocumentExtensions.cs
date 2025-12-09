using NorskHelsenettUpload.Ihe.Mhd.Models;
using NorskHelsenettUpload.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NorskHelsenettUpload.FluidExtensions;

public static class DocumentExtensions
{
	public static Document WithTitle(this Document document, string? title)
	{
		if (document == null)
			document = new Document();

		document.Title = title;
		return document;
	}

	public static Document WithFilename(this Document document, string? filename)
	{
		if (document == null)
			document = new Document();

		document.Filename = filename;
		return document;
	}

	public static Document WithSize(this Document document, int? size)
	{
		document.Size = size; 
		return document;
	}

	public static Document WithHash(this Document document, byte[]? hash)
	{
		document.Hash = hash; 
		return document;
	}

	public static Document WithMimeType(this Document document, string? mimeType)
	{	
		document.MimeType = mimeType;
		return document;
	}

	public static Document WithCreationTime(this Document document, DateTime creationTime)
	{
		document.CreationTime = creationTime; 
		return document;
	}

	public static Document WithLanguageCode(this Document document, string? languageCode)
	{	
		document.LanguageCode = languageCode;
		return document;
	}

	public static Document WithUrl(this Document document, string? url)
	{
		document.Url = url; 
		return document;
	}

	public static Document WithContent(this Document document, byte[]? content, bool computeSha)
	{
		document.Content = content; 

		if (computeSha && content != null)
		{
			var sha1 = SHA1.Create();
			
			document.Hash = sha1.ComputeHash(content);
		}
		return document;
	}
		
	public static Document WithUniqueId(this Document document, Guid uniqueId)
	{
		document.Metadata.UniqueId = uniqueId;
		return document;
	}
}

