using Microsoft.AspNetCore.Mvc.Rendering;
using NorskHelsenettUpload.Ihe.Mhd.Models;

namespace PatientHealthRecordsEPJ.UploadDocuments.Models
{
	public class ClassificationViewModel
	{	
		public Document? SelectedDocument { get; set; } = default!;
		public DocumentEntryMetadata? Metadata { get; set; } = default!;

		// Encounter information
		public SelectList SelectHealthcareFacilityList { get; set; } = default!;
		public SelectList SelectPracticeSettingCodeList { get; set; } = default!;		

		// Document classification 
		public SelectList SelectClassificationList { get; set; } = default!;
		public SelectList SelectCategoryTypeList { get; set; } = default!;
		public SelectList SelectConfidentialityCodeList_Hl7 { get; set; } = default!;
		public SelectList SelectConfidentialityCodeList_9603 { get; set; } = default!;
	}
}
