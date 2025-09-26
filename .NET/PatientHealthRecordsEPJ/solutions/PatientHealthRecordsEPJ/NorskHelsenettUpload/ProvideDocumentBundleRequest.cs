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

	//public Patient Patient { get; set; } = new Patient();
	public SourcePatientInfo? Patient { get; set; } // = new SourcePatientInfo();
	//public AuthorCollection Authors { get; set; } = new AuthorCollection();
	//public Classification Classification { get; set; } = new Classification();
	public DocumentCollection DocumentCollection { get; set; } = new DocumentCollection();
	//public Submission Submission { get; set; } = new Submission();
	public DocumentSubmissionMetadata Submission { get; set; } = new DocumentSubmissionMetadata();
	public Association? Association { get; set; } // = new Association();

	public void SetPatientOnAllDocuments()
	{
		foreach (var document in DocumentCollection.Documents)
		{
			document.Metadata.SourcePatientInfo = Patient;
		}
	}
}
