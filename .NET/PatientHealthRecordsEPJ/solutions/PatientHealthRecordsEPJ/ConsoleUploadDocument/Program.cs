using NorskHelsenettUpload;
using NorskHelsenettUpload.FluidExtensions;
using NorskHelsenettUpload.Ihe.Mhd.Models;
using NorskHelsenettUpload.Ihe.Mhd.Services;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var patientId = "111"; 

var currentDirectory = Directory.GetCurrentDirectory();


var fileName = "Epikrise.xml"; 
var filePath = Path.Combine(currentDirectory, @"Examples\", fileName); 

var fileBytes = File.ReadAllBytes(filePath);

var fileInfo = new FileInfo(filePath);

var mimeType = "application/xml"; 

var request = new NorskHelsenettUpload.ProvideDocumentBundleRequest();

request.Patient
	.WithNIN(patientId, NinType.NinTypeNationalIdentityNumber)
	.WithName(new PersonName() 
	{ 
		FirstName = "Line", 
		FamilyName = "Danser" 
	})
	.WithXdsGlobalIdentifier("1.2.3")
	.WithXdsHomeCommunity("1.2.3")
	.WithGender("F")
	.WithDateOfBirth(new DateTime(1969, 11, 13));


Document document = new Document()
			.WithFilename(fileName)
			.WithContent(fileBytes, computeSha: true)
			.WithMimeType(mimeType)
			.WithCreationTime(fileInfo.CreationTime)
			.WithSize((int)fileInfo.Length);

document.Metadata
			.WithPatientId(patientId)
			.WithSourcePatientIdToHl7String("123", NinType.NinTypeNationalIdentityNumber)
			.WithFormatCode("formatCode")
			.WithServiceStartTime(DateTime.Now.Subtract(TimeSpan.FromDays(7)))
			.WithServiceStopTime(DateTime.Now.Subtract(TimeSpan.FromDays(6)))
			;

/*
document.Metadata.WithAuthor();
document.Metadata.WithClassCode ();
document.Metadata.WithComment();
document.Metadata.WithConfidentialityCode();
document.Metadata.WithEventCode();
document.Metadata.WithFormatCode();
document.Metadata.WithHealthCareFacilityType();
document.Metadata.WithLegalAuthenticator();
document.Metadata.WithPatientId();
document.Metadata.WithPracticeSettingCode();
document.Metadata.WithServiceStartTime();
document.Metadata.WithServiceStopTime();
document.Metadata.WithSourcePatientId();
document.Metadata.WithSourcePatientInfo();
document.Metadata.WithTypeCode();
document.Metadata.WithUniqueId();
*/

request.DocumentCollection.WithDocument(document, false);

request.Submission
	.WithUniqueId(Guid.NewGuid())
	.WithTitle($"SubmissionSet-{patientId}-{DateTime.Now}")
	.WithAvailabilityStatus(DocumentSubmissionMetadata.AvailabilityStatusApproved)
	.WithSubmissionTime(DateTime.Now)
	.WithSourceId("1.2.840.4711.815.1")
	.WithPatientId(patientId)
	.WithAuthor(new Author()
	{
		Name = new NorskHelsenettUpload.Models.Xcn()
		{
			PersonIdentifier = "123",
			GivenName = "Vaske",
			FamilyName = "Maskin"
			//MiddleName = sessionData.Name,
			//Prefix
			//Suffix	
			//Degree
		}
	}	
	)
	.WithComments("Submission comment")
	;
	
	// WithSubmissionTime
	// WithAvailabilityStatus
	// WithSourceId
	// WithPatientId
	// WithComments
	// WithAuthor
	// WithAuthor
	// WithAuthor	


BundleCreator bundleCreator = new BundleCreator();

var bundleDocument = bundleCreator.CreateFhirBundle(request, "Comprehensive"); 