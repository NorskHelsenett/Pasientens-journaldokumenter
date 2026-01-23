using NorskHelsenettUpload.Ihe.Mhd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NorskHelsenettUpload; 

public class ProvideDocumentBundleRequest
{

	// Note: Every document also has a MetaData property with a SourcePatientInfo property, 
	// but since a SubmissionSet can only be for 1 single patient, then we do this to simplify: 

	// - Only manage 1 single Patient instance of type SourcePatientInfo on top-level in this class
	// - When it is time to create the FHIR bundle, then we set this instance on all documents (.MetaData.SourcePatientInfo)
	
	public SourcePatientInfo? Patient { get; set; } 	
	public DocumentCollection DocumentCollection { get; set; } = new DocumentCollection();	
	public DocumentSubmissionMetadata Submission { get; set; } = new DocumentSubmissionMetadata();
	public Association? Association { get; set; } 

	public void SetPatientOnAllDocuments()
	{
		foreach (var document in DocumentCollection.Documents)
		{
			document.Metadata.SourcePatientInfo = Patient;
		}
	}	

	public void Cleanup()
	{
		foreach (var document in DocumentCollection.Documents)
		{
			if (document.ContentLocation == Document.ContentLocationType.FilePath && File.Exists(document.ContentFilePath))
			{
				try
				{
					File.Delete(document.ContentFilePath);
				}
				catch (Exception ex)
				{
					// Log the exception or handle it as needed
					Console.WriteLine($"Error deleting file {document.ContentFilePath}: {ex.Message}");
				}
			}			
		}
	}
}
