using Microsoft.AspNetCore.Mvc.Rendering;
using NorskHelsenettUpload.Ihe.Mhd.Models;

namespace PatientHealthRecordsEPJ.UploadDocuments.Models
{
	public class DocumentMetadataViewModel
	{
		//public NorskHelsenettUpload.ProvideDocumentBundleRequest Upload { get; set; } = default!;
		//public DocumentEntryMetadata? Metadata { get; set; } = default!;
		public Document? SelectedDocument { get; set; } = default!;

		public SelectList SelectMimeTypeList { get; set; } = default!;
		public SelectList SelectCompositionFormatList { get; set; } = default!;
		public SelectList SelectLanguageList { get; set; } = default!;

		public string  Test {  get; set; }		
	}
}
