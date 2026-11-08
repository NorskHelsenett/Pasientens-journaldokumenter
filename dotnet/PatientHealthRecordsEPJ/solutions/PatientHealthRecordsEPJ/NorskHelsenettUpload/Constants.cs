namespace NorskHelsenettUpload;

public static class Constants
{

    public static class Hl7
    {
        public static class Profile
        {
            public const string MinimalProvideBundle = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.Minimal.ProvideBundle";
            public const string UncontainedComprehensiveProvideBundle = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.UnContained.Comprehensive.ProvideBundle";
            public const string ComprehensiveProvideBundle = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.Comprehensive.ProvideBundle";
        }

        public static class Dtm
        {
            /// <summary>
            /// yyyy
            /// </summary>
            public const string DtmYFormat = "yyyy";

            /// <summary>
            /// yyyyMM
            /// </summary>
            public const string DtmYmFormat = DtmYFormat + "MM";

            /// <summary>
            /// yyyyMMdd
            /// </summary>
            public const string DtmYmdFormat = DtmYmFormat + "dd";

            /// <summary>
            /// yyyyMMddHH
            /// </summary>
            public const string DtmYmdhFormat = DtmYmdFormat + "HH";

            /// <summary>
            /// yyyyMMddHHmm
            /// </summary>
            public const string DtmYmdhmFormat = DtmYmdhFormat + "mm";

            /// <summary>
            /// yyyyMMddHHmmss
            /// </summary>
            public const string DtmFormat = DtmYmdhmFormat + "ss";

            /// <summary>
            /// yyMMddHHmmss
            /// </summary>
            public const string DtmYyFormat = "yyMMddHHmmss";

            /// <summary>
            /// yyyyMMddHHmmssfff
            /// </summary>
            public const string DtmLongFormat = DtmFormat + "fff";

            public const string DtmFhirIsoDateTimeFormat = "yyyy-MM-ddTHH:mm:ssK";

            public const string DtmFhirIsoDateFormat = "yyyy-MM-dd";

            public static readonly string[] AllFormats =
            [
                "yyyy-MM-ddTHH:mm:ssK",
                "yyyyMMddHHmmss.FFFFzzz",
                "yyyyMMddHHmmss.FFFF",
                "yyyyMMddHHmmsszzz",
                "yyyyMMddHHmmss",
                "yyyyMMddHHmm.FFFFzzz",
                "yyyyMMddHHmm.FFFF",
                "yyyyMMddHHmmzzz",
                "yyyyMMdd",
                "yyyyddMM",
                "yyyyMM",
                "yyyy"
            ];

        }

        public static class Namespaces
        {
            public const string Hl7V3 = "urn:hl7-org:v3";
            public const string Hl7Sdtc = "urn:hl7-org:sdtc";
        }

        public static class Separator
        {
            public const char Amp = '&';
            public const char Hatt = '^';
        }

        public static class UniversalIdType
        {
            //http://www.hl7.eu/refactored/tab0301.html
            public const string Iso = "ISO";

            public const string Uuid = "UUID";
            public const string Guid = "GUID";
            public const string Dns = "DNS";
        }

        public static class Pid
        {
            public const string PidBase = "PID-";
        }

        public static class CodeSystems
        {
            public const string IsoHealthRecordLifecycleEvent = "http://terminology.hl7.org/CodeSystem/iso-21089-lifecycle";
        }
    }

	public static class CodeSystems
	{
		public static class Volven
		{
			public const string Gender = "2.16.578.1.12.4.1.1.3101";
			public const string DocumentType = "2.16.578.1.12.4.1.1.9602";
			public static class ConfidentialityCode
			{
				public const string Oid = "2.16.578.1.12.4.1.1.9603";

				/// <summary> Normal</summary>
				public const string N = "N";
				/// <summary> Nektet, alle dokumenter</summary>
				public const string NORN_ALL = "NORN_ALL";
				/// <summary> Nektet, duplikat</summary>
				public const string NORN_DUP = "NORN_DUP";
				/// <summary> Nektet, eget ønske</summary>
				public const string NORN_EPO = "NORN_EPO";
				/// <summary> Nektet, fare for helsepersonell</summary>
				public const string NORN_FFH = "NORN_FFH";
				/// <summary> Nektet, fare for liv</summary>
				public const string NORN_FFL = "NORN_FFL";
				/// <summary> Nektet, foreldet</summary>
				public const string NORN_FOR = "NORN_FOR";
				/// <summary> Nektet, foreldreansvarlig</summary>
				public const string NORN_FORANS = "NORN_FORANS";
				/// <summary> Nektet, forsvarlig pasientbehandling</summary>
				public const string NORN_FPB = "NORN_FPB";
				/// <summary> Nektet, klart utilrådelig</summary>
				public const string NORN_KUT = "NORN_KUT";
				/// <summary> Nektet, ungdom</summary>
				public const string NORN_UNGDOM = "NORN_UNGDOM";
				/// <summary> Sperret</summary>
				public const string NORS = "NORS";
				/// <summary> Utsatt innsyn for innbygger</summary>
				public const string NORU = "NORU";
				/// <summary> Nektet, andre grunner</summary>
				public const string NORN_ANG = "NORN_ANG"; // Inaktiv fra 14.12.2025, men allikevel fortsatt brukt i NHN dokumentlager inntil videre 

			}

			public const string EventCode = "2.16.578.1.12.4.1.1.7210";


		}

		public static class Hl7
		{
			public static class ConfidentialityCode
			{
				public const string Oid = "2.16.840.1.113883.5.25";

				/// <summary>low</summary>
				public const string Low = "L";
				/// <summary>moderate</summary>
				public const string Moderate = "M";
				/// <summary>normal</summary>
				public const string Normal = "N";
				/// <summary>restricted</summary>
				public const string Restricted = "R";
				/// <summary>unrestricted</summary>
				public const string Unrestricted = "U";
				/// <summary>veryrestricted</summary>
				public const string VeryRestricted = "V";

			}
			public static class AuditEventId
			{
				public const string Oid = "2.16.840.1.113883.4.642.3.462";
			}

			public static class PurposeOfUse
			{
				public const string Oid = "2.16.840.1.113883.1.11.20448";
				/// <summary>healthcare marketing</summary>
				public const string HMARKT = "HMARKT";
				/// <summary>healthcare operations</summary>
				public const string HOPERAT = "HOPERAT";
				/// <summary>care management</summary>
				public const string CAREMGT = "CAREMGT";
				/// <summary>donation</summary>
				public const string DONAT = "DONAT";
				/// <summary>fraud</summary>
				public const string FRAUD = "FRAUD";
				/// <summary>government</summary>
				public const string GOV = "GOV";
				/// <summary>health accreditation</summary>
				public const string HACCRED = "HACCRED";
				/// <summary>health compliance</summary>
				public const string HCOMPL = "HCOMPL";
				/// <summary>decedent</summary>
				public const string HDECD = "HDECD";
				/// <summary>directory</summary>
				public const string HDIRECT = "HDIRECT";
				/// <summary>healthcare delivery management</summary>
				public const string HDM = "HDM";
				/// <summary>legal</summary>
				public const string HLEGAL = "HLEGAL";
				/// <summary>health outcome measure</summary>
				public const string HOUTCOMS = "HOUTCOMS";
				/// <summary>health program reporting</summary>
				public const string HPRGRP = "HPRGRP";
				/// <summary>health quality improvement</summary>
				public const string HQUALIMP = "HQUALIMP";
				/// <summary>health system administration</summary>
				public const string HSYSADMIN = "HSYSADMIN";
				/// <summary>labeling</summary>
				public const string LABELING = "LABELING";
				/// <summary>metadata management</summary>
				public const string METAMGT = "METAMGT";
				/// <summary>member administration</summary>
				public const string MEMADMIN = "MEMADMIN";
				/// <summary>military command</summary>
				public const string MILCDM = "MILCDM";
				/// <summary>patient administration</summary>
				public const string PATADMIN = "PATADMIN";
				/// <summary>patient safety</summary>
				public const string PATSFTY = "PATSFTY";
				/// <summary>performance measure</summary>
				public const string PERFMSR = "PERFMSR";
				/// <summary>records management</summary>
				public const string RECORDMGT = "RECORDMGT";
				/// <summary>system development</summary>
				public const string SYSDEV = "SYSDEV";
				/// <summary>test health data</summary>
				public const string HTEST = "HTEST";
				/// <summary>training</summary>
				public const string TRAIN = "TRAIN";
				/// <summary>healthcare payment</summary>
				public const string HPAYMT = "HPAYMT";
				/// <summary>claim attachment</summary>
				public const string CLMATTCH = "CLMATTCH";
				/// <summary>coverage authorization</summary>
				public const string COVAUTH = "COVAUTH";
				/// <summary>coverage under policy or program</summary>
				public const string COVERAGE = "COVERAGE";
				/// <summary>eligibility determination</summary>
				public const string ELIGDTRM = "ELIGDTRM";
				/// <summary>eligibility verification</summary>
				public const string ELIGVER = "ELIGVER";
				/// <summary>enrollment</summary>
				public const string ENROLLM = "ENROLLM";
				/// <summary>military discharge</summary>
				public const string MILDCRG = "MILDCRG";
				/// <summary>remittance advice</summary>
				public const string REMITADV = "REMITADV";
				/// <summary>healthcare research</summary>
				public const string HRESCH = "HRESCH";
				/// <summary>biomedical research</summary>
				public const string BIORCH = "BIORCH";
				/// <summary>clinical trial research</summary>
				public const string CLINTRCH = "CLINTRCH";
				/// <summary>clinical trial research without patient care</summary>
				public const string CLINTRCHNPC = "CLINTRCHNPC";
				/// <summary>clinical trial research with patient care</summary>
				public const string CLINTRCHPC = "CLINTRCHPC";
				/// <summary>preclinical trial research</summary>
				public const string PRECLINTRCH = "PRECLINTRCH";
				/// <summary>disease specific healthcare research</summary>
				public const string DSRCH = "DSRCH";
				/// <summary>population origins or ancestry healthcare research</summary>
				public const string POARCH = "POARCH";
				/// <summary>translational healthcare research</summary>
				public const string TRANSRCH = "TRANSRCH";
				/// <summary>patient requested</summary>
				public const string PATRQT = "PATRQT";
				/// <summary>family requested</summary>
				public const string FAMRQT = "FAMRQT";
				/// <summary>power of attorney</summary>
				public const string PWATRNY = "PWATRNY";
				/// <summary>support network</summary>
				public const string SUPNWK = "SUPNWK";
				/// <summary>public health</summary>
				public const string PUBHLTH = "PUBHLTH";
				/// <summary>disaster</summary>
				public const string DISASTER = "DISASTER";
				/// <summary>threat</summary>
				public const string THREAT = "THREAT";
				/// <summary>treatment</summary>
				public const string TREAT = "TREAT";
				/// <summary>clinical trial</summary>
				public const string CLINTRL = "CLINTRL";
				/// <summary>coordination of care</summary>
				public const string COC = "COC";
				/// <summary>Emergency Treatment</summary>
				public const string ETREAT = "ETREAT";
				/// <summary>break the glass</summary>
				public const string BTG = "BTG";
				/// <summary>emergency room treatment</summary>
				public const string ERTREAT = "ERTREAT";
				/// <summary>population health</summary>
				public const string POPHLTH = "POPHLTH";
			}
		}

		public static class OtherIsoDerived
		{
			public static class PurposeOfUse
			{
				public const string Oid = "1.0.14265.1";
				public const string ClinicalCare_1 = "1";
				public const string EmergencyCare_2 = "2";
				public const string Management_5 = "5";
				public const string SubjectOfCare_13 = "13";
			}
		}
	}

	public static class Oid
    {
        // The correct "system"-value for OID
        public const string System = "urn:ietf:rfc:3986";

        public const string Fnr = "2.16.578.1.12.4.1.4.1";
        public const string Dnr = "2.16.578.1.12.4.1.4.2";
        public const string Hnr = "2.16.578.1.12.4.1.4.3";
        public const string Hpr = "2.16.578.1.12.4.1.4.4";
        public const string ReshId = "2.16.578.1.12.4.1.4.102";
        public const string Brreg = "2.16.578.1.12.4.1.4.101";
        public const string Nhn = "2.16.578.1.12.4.5";        

        public static class Saml
        {
            public static class Acp
            {
                // Citizen OID values

                /// <summary>
                /// CUSTOM OID: No representation overrides (represents themself)
                /// </summary>
                public const string NullValue = "urn:oid:2.16.578.1.12.4.1.7.2.1.0";

                /// <summary>
                /// Represent citizen under 12 years of age
                /// </summary>
                public const string RepresentCitizenUnder12 = "urn:oid:2.16.578.1.12.4.1.7.2.1.1";

                /// <summary>
                /// Represent another cititzen (Power of Attorney)
                /// </summary>
                public const string RepresentAnotherCitizen = "urn:oid:2.16.578.1.12.4.1.7.2.1.2";

                /// <summary>
                /// Represent citizen unable to give consent
                /// </summary>
                public const string RepresentedUnableToConsent = "urn:oid:2.16.578.1.12.4.1.7.2.1.3";

                // Healthcare practitioner OID values

                /// <summary>
                /// Healthcare professional [subject] is not obliged to retrieve patient's consent to [resource] open and see patient's healthcare data, e.g. "patient's regular physician" (fastlege)
                /// </summary>
                public const string NotObligedToConsent = "urn:oid:2.16.578.1.12.4.1.7.2.1.4";

                /// <summary>
                /// Healthcare professional [subject] has been given explicit consent from patient [resource] to open and see patient's healthcare data, including locked data
                /// </summary>
                public const string ExcplicitConsent = "urn:oid:2.16.578.1.12.4.1.7.2.1.5";

                /// <summary>
                /// Healthcare professional [subject] is not able to retrieve consent from current patient [resource] (e.g. patient is unconscious)
                /// </summary>
                public const string UnableToConsent = "urn:oid:2.16.578.1.12.4.1.7.2.1.6";

                /// <summary>
                /// Healthcare professional [subject] has documented reasons to unlock all available healthcare data for current patient [resource] in an emergency/catastrophic situation
                /// </summary>
                public const string ExceptionToConcent = "urn:oid:2.16.578.1.12.4.1.7.2.1.7";

                /// <summary>
                /// Healthcare professional [subject] has retrieved consent from patient [resource] to open and see patient's healthcare data
                /// </summary>
                public const string HasConsent = "urn:oid:2.16.578.1.12.4.1.7.2.1.8";

            }

            public static class Bppc
            {
                /// <summary>
                /// CUSTOM OID: Null value
                /// </summary>
                public const string NullValue = "urn:oid:2.16.578.1.12.4.1.7.2.2.0";

                /// <summary>
                /// Consent from an analog channel
                /// </summary>
                public const string AnalogChannel = "urn:oid:2.16.578.1.12.4.1.7.2.2.1";

                /// <summary>
                /// Consent from a digital channel
                /// </summary>
                public const string DigitalChannel = "urn:oid:2.16.578.1.12.4.1.7.2.2.2";
            }

        }
    }
    public static class MimeTypes
    {
        public const string FhirJson = "application/fhir+json";
        public const string Hl7v3Xml = "application/hl7-v3+xml";
        public const string Json = "application/json";
        public const string SoapXml = "application/soap+xml";
        public const string MultipartRelated = "multipart/related";
        public const string XopXml = "application/xop+xml";
        public const string SevenZip = "application/x-7z-compressed";
        public const string Acc = "audio/aac";
        public const string Avi = "video/x-msvideo";
        public const string Doc = "application/msword";
        public const string Docm = "application/vnd.ms-word.document.macroEnabled.12";
        public const string Docx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public const string Gif = "image/gif";
        public const string Html = "text/html";
        public const string Jpeg = "image/jpeg";
        public const string Mp4 = "video/mp4";
        public const string Mpeg = "video/mpeg";
        public const string Odp = "application/vnd.oasis.opendocument.presentation";
        public const string Ods = "application/vnd.oasis.opendocument.spreadsheet";
        public const string Odt = "application/vnd.oasis.opendocument.text";
        public const string Oga = "audio/ogg";
        public const string Ogv = "video/ogg";
        public const string Pdf = "application/pdf";
        public const string Png = "image/png";
        public const string Pps = "application/vnd.ms-powerpoint";
        public const string Ppsm = "application/vnd.ms-powerpoint.slideshow.macroEnabled.12";
        public const string Ppt = "application/vnd.ms-powerpoint";
        public const string Pptm = "application/vnd.ms-powerpoint.presentation.macroEnabled.12";
        public const string Pptx = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
        public const string Rtf = "application/rtf";
        public const string Text = "text/plain";
        public const string Tiff = "image/tiff";
        public const string Binary = "application/octet-stream";
        public const string Vsd = "application/vnd.visio";
        public const string Wav = "audio/x-wav";
        public const string Weba = "audio/webm";
        public const string Webm = "video/webm";
        public const string Webp = "image/webp";
        public const string Xhtml = "application/xhtml+xml";
        public const string Xls = "application/vnd.ms-excel";
        public const string Xlsb = "application/vnd.ms-excel.sheet.binary.macroEnabled.12";
        public const string Xlsm = "application/vnd.ms-excel.sheet.macroEnabled.12";
        public const string Xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string Xml = "application/xml";
        public const string XmlReadable = "text/xml";
        public const string Zip = "application/zip";
    }
}    

/*public static class ConstantsExtensions
{
    public static Dictionary<string, string> GetAsDictionary(this Type type)
    {
        var constants = new Dictionary<string, string>();

        // Get all static fields of the class
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            var value = (string?)field.GetValue(null);
            // Ensure that the field is a constant (it should be a static readonly or const field)
            if (field.IsLiteral && !field.IsInitOnly && value != null)
            {
                constants.Add(field.Name, value);
            }
        }

        return constants;
    }
    public static List<KeyValueEntry> GetAsKeyValuePair(this Type type)
    {
        var constants = new List<KeyValueEntry>();

        // Get all static fields of the class
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);

        foreach (var field in fields)
        {
            var value = (string?)field.GetValue(null);

            // Ensure that the field is a constant (it should be a static readonly or const field)
            if (field.IsLiteral && !field.IsInitOnly && value != null)
            {
                constants = [.. constants, new KeyValueEntry() { Key = field.Name, Value = value }];
            }
        }

        return constants;
    }
}
*/