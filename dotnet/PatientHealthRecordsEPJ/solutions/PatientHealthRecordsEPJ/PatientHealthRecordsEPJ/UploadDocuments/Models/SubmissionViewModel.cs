using Microsoft.AspNetCore.Mvc.Rendering;

namespace PatientHealthRecordsEPJ.UploadDocuments.Models
{
	public class SubmissionViewModel
	{
		//public NorskHelsenettUpload.ProvideDocumentBundleRequest Upload { get; set; } = default!;
		public NorskHelsenettUpload.Ihe.Mhd.Models.DocumentSubmissionMetadata Submission { get; set; } = default!;

		public SelectList SelectAvailabilityStatusList { get; set; } = default!;

		public string AuthorName { get; set; }
	}
}
