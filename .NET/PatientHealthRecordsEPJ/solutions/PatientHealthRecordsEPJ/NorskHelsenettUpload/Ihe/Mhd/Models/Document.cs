using Hl7.Fhir.Utility;
using System.ComponentModel.DataAnnotations;

namespace NorskHelsenettUpload.Ihe.Mhd.Models;

public enum CustomDocumentRelationshipType // The same as Hl7.Fhir.Model.DocumentRelationshipType, but with added ReplaceAndTransform, SnapshotOf and NoRelation
{	
	Replaces,	
	Transforms,	
	Signs,	
	Appends,
    ReplaceAndTransform,
    SnapshotOf,
    NoRelation
}

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

	public string? ExistingDocumentId { get; set; } // When using relatesTo for replace, transform, add to existing, sign, snapshot, this points to the existing document Id and will be saved in relatesTo.TargetObject
    public CustomDocumentRelationshipType? DocumentRelationshipType { get; set; } // When using relatesTo for replace, transform, add to existing, sign, snapshot, this points to the relationship type and will be saved in relatesTo.TypeCode

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
        this.ExistingDocumentId = document.ExistingDocumentId;
        this.DocumentRelationshipType = document.DocumentRelationshipType;
		//this.Metadata = document.Metadata;
	}
}
