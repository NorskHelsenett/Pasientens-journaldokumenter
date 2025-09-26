using Microsoft.AspNetCore.Mvc.Rendering;

namespace PatientHealthRecordsEPJ.UploadDocuments.Models
{
	public class ClassificationViewModel
	{
		//public NorskHelsenettUpload.ProvideDocumentBundleRequest BundleRequest { get; set; } = default!; 
		public NorskHelsenettUpload.Ihe.Mhd.Models.DocumentEntryMetadata? Metadata { get; set; } = default!;

		// Encounter information
		public SelectList SelectHealthcareFacilityList { get; set; } = default!;
		public SelectList SelectPracticeSettingCodeList { get; set; } = default!;

		public string  Test {  get; set; }


		// Document classification 
		public SelectList SelectClassificationList { get; set; } = default!;
		public SelectList SelectCategoryTypeList { get; set; } = default!;
		public SelectList SelectConfidentialityCodeList { get; set; } = default!;
	}
}
