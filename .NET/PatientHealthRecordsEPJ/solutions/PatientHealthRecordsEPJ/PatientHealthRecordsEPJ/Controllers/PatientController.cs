using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NorskHelsenettUpload.FluidExtensions;
using NorskHelsenettUpload.Ihe.Mhd.Models;
using PatientHealthRecordsEPJ.Models;
using PatientHealthRecordsEPJ.Services;

namespace PatientHealthRecordsEPJ.Controllers;

public class PersontjenestenSearchParams
{
	public string? NIN { get; set; }
}

public class PatientController : BaseController
{
	private readonly ILogger<HomeController> _logger;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private ISession session => _httpContextAccessor.HttpContext!.Session;	
	private readonly PersonApiService _personApiService; 	

	public PatientController(
		IOptions<SessionOptions> sessionOptions,
		ILogger<HomeController> logger,
		IHttpContextAccessor httpContextAccessor,		
		PersonApiService personApiService,
		IUserTokenService userTokenService
		) : base(sessionOptions, userTokenService)
	{
		_logger = logger;
		_httpContextAccessor = httpContextAccessor;
		_personApiService = personApiService; 
	}

	public IActionResult Pasientvalg()
	{
		PasientvalgViewModel model = new PasientvalgViewModel();

		model.EnvironmentId = "test";
		model.GrunnlagId = "samtykke";

		var sessionLoginResult = session.SessionData();

		model.PatientId = sessionLoginResult.SourcePatientInfo?.NIN ?? string.Empty;		

		if (string.IsNullOrEmpty(model.PatientId))
		{
			//model.PatientId = "12119000465"; // Malin Fos Bogen som standard
			//model.PatientId = "13116900216"; // Line Danser som standard

			//model.PatientId = "22876699307"; // LIVSGLAD KREPS
		}

		return View(model);
	}

	[HttpPost]
	public async Task<IActionResult> VelgPasient(PasientvalgViewModel model, string nin)
	{
		var sessionData = session.SessionData();

		if (!string.IsNullOrEmpty(nin))
		{
			model.PatientId = nin; 
		}

		return RedirectToAction(nameof(ChoosePasient), new { patientId = model.PatientId, grunnlag = model.GrunnlagId });		
	}
	
	public async Task<IActionResult> ChoosePasient(string patientId, string grunnlag)
	{
		// Method is also called from HomeController if client sends query parameter ?patientIdentifier=... or ?pid=...

		var sessionData = session.SessionData();
		
		var patient = await FindPersonAsync(patientId);

		if (patient == null)
		{
			patient = new SourcePatientInfo()
			{
				NIN = patientId
			};

			// Ikke syntetisk patient (e.g. Malin Fos Bogen)
			var nonSyntpopPatient = PasientvalgViewModel.PatientMappingList.FirstOrDefault(x => x.Id == patientId);

			if (nonSyntpopPatient != null)
			{
				var firstName = nonSyntpopPatient.Label.Substring(0, nonSyntpopPatient.Label.IndexOf(" "));
				var lastName = nonSyntpopPatient.Label.Substring(nonSyntpopPatient.Label.IndexOf(" ") + 1);
				patient = patient.WithName(new PersonName() { FirstName = firstName, FamilyName = lastName });
			}
		}

		PersontjenestenSearchParams search = new();

		sessionData.SourcePatientInfo = patient;
		sessionData.PatientId = patientId;
		sessionData.PatientName = patient?.Name.ToString();

		sessionData.GrunnlagId = grunnlag;
		sessionData.GrunnlagName = PasientvalgViewModel.GrunnlagList.FirstOrDefault(x => x.Id == grunnlag)?.Label;
		//sessionLoginResult.EnvironmentId = model.EnvironmentId;
		//sessionLoginResult.EnvironmentName = PasientvalgViewModel.EnvironmentList .FirstOrDefault(x => x.Id == model.EnvironmentId)?.Label;

		// Change patient in provide document bundle submission set, if there is one

		if (sessionData.BundleRequest != null)
		{
			var bundleRequest = sessionData.BundleRequest;

			bundleRequest.Submission
				.WithPatientId(sessionData.PatientId);

			bundleRequest.Patient = sessionData
				.SourcePatientInfo!
				.WithXdsGlobalIdentifier("1.2.3") // If we fix/change this, also fix in UploadDocumentsController
				.WithXdsHomeCommunity("1.2.3")
				;
		}

		sessionData.SaveToSession(session);

		return Redirect("/");
	}

	[HttpPost]
	public IActionResult SearchNIN(PasientvalgViewModel model, string nin)
	{
		return View();
	}

	private async Task<SourcePatientInfo?> FindPersonAsync(string nin)
	{
		SourcePatientInfo? patient = new SourcePatientInfo();		
				
		string[] informationParts = ["Name", "Gender", "Birth", "AddressProtection", "NorwegianIdentificationNumber"];

		var person = await _personApiService.FullAccess.GetByNin(nin, informationParts);

		if (person == null)
		{
			return null!; 
		}

		//var patient = await ApiService.GetPersonByPostAsync(search.NIN!);

		patient.NIN = person!.norwegianIdentificationNumber.LastOrDefault()!.identificationNumber;
		patient.NinType = person!.norwegianIdentificationNumber.LastOrDefault()!.identificationNumberType;

		patient.Name.FamilyName = person!.name.LastOrDefault()!.familyName;
		patient.Name.FirstName = person!.name.LastOrDefault()!.givenName;
		if (person!.name.LastOrDefault()!.middleName != null)
		{
			patient.Name.MiddleName = person!.name.LastOrDefault()!.middleName.ToString();
		}
		else patient.Name.MiddleName = "";

		patient.Gender = person!.gender.LastOrDefault()!.gender;
		patient.DateOfBirth = person!.birth.LastOrDefault()!.birthDate;		

		return patient;
	}

	private async Task<Hl7.Fhir.Model.Patient> FindPersonReferenceInXdsAsync(SourcePatientInfo person)
	{
		//var patient = await ApiService.GetPixReferenceForPerson(person.NIN!);

		//return patient!;
		return null; 
	} 

	// Registers current patient-context for application
	/*private void SelectPatient(SourcePatientInfo patient)
	{
		var state = PatientContextHandler.UpsertPatientSessionData(userSessionData.Identities.FirstOrDefault()!.Name!, patient);

		if (state == Task.CompletedTask)
		{
			ShowMessage(ToastType.Info, "Patient context selected");
			//statusTask = "Completed"; 
		}
		;
	}*/

	// Calculates age for given person-object
	private int GetAge(DateTime birthDate)
	{
		int age = 0;

		var today = DateTime.Today;

		var a = (today.Year * 100 + today.Month) * 100 + today.Day;
		var b = (birthDate.Year * 100 + birthDate.Month) * 100 + birthDate.Day;

		age = (a - b) / 10000;

		return age;
	}
}