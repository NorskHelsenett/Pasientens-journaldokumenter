using NorskHelsenettUpload.Ihe.Mhd.Models;

namespace NorskHelsenettUpload; 

public class DocumentCollection
{
	//public Document? SelectedDocument { get; set; }     // The document which is currently selected in the UI for editing
	public Guid? SelectedDocumentUniqueId { get; set; }     // The document which is currently selected in the UI for editing

	public List<Document> Documents { get; set; } = new List<Document>();

	public Document? SelectedDocument 
	{ 
		get
		{
			return SelectedDocumentUniqueId != null ? 
				Documents.First(document => document.Metadata.UniqueId == SelectedDocumentUniqueId)
				: null;
		}		
	}

	public DocumentCollection WithDocument(Document document, bool selectDocument)
	{
		Documents.Add(document);

		if (selectDocument)
		{
			//SelectedDocument = document;
			SelectedDocumentUniqueId = document.Metadata.UniqueId;
		}
		return this; 
	}

	public Document? SelectDocument(Guid uniqueID)
	{
		SelectedDocumentUniqueId = uniqueID;

		return SelectedDocument; 
	}

	public Document? RemoveDocument(Guid uniqueID)
	{
		Documents.RemoveAll(document => document.Metadata.UniqueId == uniqueID);

		if (SelectedDocumentUniqueId == uniqueID)
		{
			var firstDocument = Documents.FirstOrDefault();

			SelectedDocumentUniqueId = firstDocument?.Metadata.UniqueId ?? null; 
		}
		
		return SelectedDocument;
	}

	public void Clear()
	{
		Documents.Clear();

		SelectedDocumentUniqueId = null; 			
	}
}
