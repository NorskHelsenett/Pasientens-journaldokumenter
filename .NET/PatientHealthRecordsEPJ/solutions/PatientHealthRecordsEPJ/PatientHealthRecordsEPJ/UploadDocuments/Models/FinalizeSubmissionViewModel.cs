using Microsoft.AspNetCore.Mvc.Rendering;

namespace PatientHealthRecordsEPJ.UploadDocuments.Models
{
	public class FinalizeSubmissionViewModel
	{
		public NorskHelsenettUpload.ProvideDocumentBundleRequest Upload { get; set; } = default!;

		public string? JsonBundleDocument { get; set; }

		public bool IsError { get; set; }
		public string ErrorMessage { get; set; }

		public string? ResponseBody { get; set; }

        public string? RegistryRepositoryBaseUrl { get; set; }

		public bool IsValidate { get; set; }
		public bool ValidationSuccess { get; set; }
		public string? ValidationMessage { get; set; }

	}
}
