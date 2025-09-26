using System.ComponentModel.DataAnnotations;

namespace NorskHelsenettUpload.Ihe.Mhd.Models;

public class Document
{
    public string? Title { get; set; }
	public string? Filename { get; set; }
	[Required]
    public int? Size { get; set; }
    [Required]
    public byte[]? Hash { get; set; }
    
    [Required]
    public string? MimeType { get; set; }
    [Required]
    public DateTime CreationTime { get; set; }
    [Required]
    public string? LanguageCode { get; set; }
    // Content might be specified either by file.Content or file.Url
    public string? Url { get; set; }
    public byte[]? Content { get; set; }
    [Required]
    public DocumentEntryMetadata Metadata { get; set; } = new DocumentEntryMetadata();	

    public void Update(Document document)
    {
        this.Title = document.Title;
        this.Filename = document.Filename;
        this.Size = document.Size;
        this.Hash = document.Hash;
        this.MimeType = document.MimeType;
        this.Url = document.Url;        
        this.CreationTime = document.CreationTime;
        this.LanguageCode = document.LanguageCode;
        this.Url = document.Url;
        //this.Metadata = document.Metadata;
    }
}
