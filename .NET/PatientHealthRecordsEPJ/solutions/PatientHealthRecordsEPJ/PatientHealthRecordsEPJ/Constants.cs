using System.Text;
using System.Xml;

public class Constants
{
	public const string SessionDataKey = "sessionData";	

	public const long KB = 1024;
	public const long MB = KB * 1024;
	public const long GB = MB * 1024;

	// Dette er privatnøkkelen til 2141d734-7604-42ca-a377-27f1b712f4e6 - "Patient Health Records API EPJ"
	public const string JwkPrivateKey = """ 
                {
            "p": "_8WHFd_pMbnVMwNVn1zxuK3SOrTb0yQyq6j3yQ2bTkOGnZ3Qqh760fDcyDrW4h_Hhy2ssbPbuwyNFf9j5boDQ1R3IZL8hFq-dkF55pHGRMSBt0OVQYWu79mfU8KVaeYi439N9Hgt_XRl5J8VOaxN2sx1tWe4Gbjy9z6IEyJ8LgM",
            "kty": "RSA",
            "q": "iRR5niXtUmxH2cUNQCq5WhzMEvpAC5BbgBEyG0KjTX7RflbJmNflb-lwwLSNsZROGSAHra3rYvA5oXfsbdlIbkOmOpV6AeySAwYPjTpp5E2qx4Jnc7v23Fl9hAfeQLJp3Gsg1lFs1COUXZmvzA06mI2Xor5gyHiTC1MOjmu4CZ0",
            "d": "EJ2l3QwmSiehnBHSdagMpk1_TomKLS9aMp91lJHXJpm6NAg_WWml0S41uE1rZE0veQBIeODeLtV9PR4J6S46XanqKiHGT4os_WLJIMD4yZGH05Hbot2jb9rfif5uRYVJ4oEBAaZdTmr0UFDqAn3PCwx2YziW6EOrAeQU5-XLFPlvFhscLJhTonZpDL6soQBMyJUV57a6-6Lsm3iNxbCd-x_DaZSyS4iB0RxBXGwyvWPL8846L6Ch87Tt2PAa1lVRb4YZgCmOiar9oPtRe6GI2lbyNGtVkxi6UXGpGMdeyXRRRhfuGGmdq0z1RWTl_fw32Y6vHNnrLhk66LPha-pgSQ",
            "e": "AQAB",
            "use": "sig",
            "kid": "oIAHPtvFo3z2jc1NV4x8FUOTGkDhyer2o6_9cxWV72o",
            "qi": "30xJ0V11tvtVI1--woxliXKx9aUpNZ3zQ0KgmiAZSvP2Y6bNccLwMnb5I8ctLocRiazeIrvNT7LwB4Kdc_hCyE0TxqC9pWKevMtzwVKReSRmO2Zp7OGty1gzSpd7ZmZNbJGLpCWvKkB34AiVJLvq9Xk9B7xB8oQ0oPMYjfNklA8",
            "dp": "XQ1dBw1ORqQIN6X36aGj43-Bo5AXBar-sEsp0sCbhw60E_XNa4cX03dgq4wUj3HmYnywfnoj79oyHhWrH3HcLAu3x8Q1EGX_MwDBO6w_SNeBJc51p7_eFC7Fc3CwcTWj7cZr8wwiFzrYsyNZUoEoVfjaviO868bIxkC-us9qBEc",
            "alg": "RS256",
            "dq": "OKiN3CAxB3BaAljAMVV3ZxegAfgNoLn6F-UDoODaPp28WUvl55bU7ny-begy6MMzxI7InbDVKf_A0kyPHJhCp9CuVGiUFVeCwl0u8GDEg-jWPcBYoi1-y3TxvDDEXFliCAj_eZYONWC_YjCzyNNu9L8RfeLSfgnYPUYHLja3ysE",
            "n": "iPUqO6EwqOdtCtUIpO40basdlyD18hNc412alGkVKvJ4ieIJZAEiYY7z0FIHVPytKZ-FLD_6Ut10aZVlXSanfIQAxjQ89Z2v0Jk1-toGU_sFq4EeEDQ7zlyk4KvQFsgN5wsoG0hxOwafvTK4mYKsaVVWLoYzvHgB-2YPANPTJE8RQZ6juLWmdt2S0yGW2bRW2kMSf8Rpnaj_ufGFudTtNbLTdVzbhaoMZJ_YnOOyX8b6u5E2hN3ucWxCpnsseEmHcWYP5CyvybrlI6KlQDo4tvweziZ_86S_nbeJJa56CW_DcEA9tdvLmi_q6g2K_Dhe7ndb7uFHrfqYq4AY1u5S1w"
        }
        """;

	public static class Oid
	{
		public const string DirektoratetForEhelse = "2.16.578.1.12.4.1";
		public const string HelseMidtNorgeRhf = "2.16.578.1.12.4.2";
		public const string HelseSorOstRhf = "2.16.578.1.12.4.3";
		public const string HelseNordRhf = "2.16.578.1.12.4.4";
		public const string HelseVestRhf = "2.16.578.1.34";
		public const string NorskHelseNett = "2.16.578.1.12.4.5";

		/// <summary>
		///     2.16.578.1.12.4.1.4
		///     Hver OID under denne noden representerer en identifikasjonsserie for personer, organisasjoner etv
		/// </summary>
		public const string Identifikasjonsserie = "2.16.578.1.12.4.1.4";

		public const string Fodselsnummer = "2.16.578.1.12.4.1.4.1";
		public const string DNummer = "2.16.578.1.12.4.1.4.2";
		public const string FellesHjelpenummer = "2.16.578.1.12.4.1.4.3";
		public const string HprNummer = "2.16.578.1.12.4.1.4.4";
		public const string DufNummer = "2.16.578.1.12.4.1.4.5";
		public const string OrganisasjonNummer = "2.16.578.1.12.4.1.4.101";
		public const string ReshId = "2.16.578.1.12.4.1.4.102";
		public const string ApotekKonsesjonsnummer = "2.16.578.1.12.4.1.4.107";

		public const string Department_NHN_Custom_Oid_Created_June_2026 = "2.16.578.1.12.4.5.390"; 

		/// <summary>
		///     2.16.578.1.12.4.1.2
		///     Identifikator for kommunikasjonsparter i helsenettet.
		///     Norsk Helsenett foretar tildelinger under denne noden
		/// </summary>
		public const string HerId = "2.16.578.1.12.4.1.2";

		/// <summary>
		///     2.16.578.1.12.4.1.6
		///     Hver OID under denne noden representerer en EPJ-komponenttype
		/// </summary>
		public const string EpjKomponentType = "2.16.578.1.12.4.1.6";

		public const string EpjDataelementType = "2.16.578.1.12.4.1.6.11";
		public const string EpjFragmentType = "2.16.578.1.12.4.1.6.12";
		public const string EpjDokumentType = "2.16.578.1.12.4.1.6.13";
		public const string EpjSaksType = "2.16.578.1.12.4.1.6.14";

		/// <summary>
		///     2.16.578.1.12.4.1.1
		///     Kodeverk publisert på volven.no
		/// </summary>
		public const string KodeverkVolven = "2.16.578.1.12.4.1.1";

		public const string AnatomiskLokalisasjon = "2.16.578.1.12.4.1.1.8352";
		public const string NorskLaboratoriekodever = "2.16.578.1.12.4.1.1.7280";
		public const string Provemateriale = "2.16.578.1.12.4.1.1.8351";

		public const string ConfidentialityCode = "2.16.840.1.113883.5.25";
		public const string CdaRootTypeId = "2.16.840.1.113883.1.3";

		public static class MockEhelse
		{
			public const string Helsenorge = DirektoratetForEhelse + ".99";
			public const string HelsenorgeRepTyper = Helsenorge + ".50";
			public const string HelsenorgeRepForelder = HelsenorgeRepTyper + ".1";
			public const string HelsenorgeRepFullmakt = HelsenorgeRepTyper + ".2";
			public const string HelsenorgeRepKilder = Helsenorge + ".70";
			public const string HelsenorgeRepKildeHn = HelsenorgeRepKilder + ".1";
		}
	}

	public static class Saml
	{
		public static class Attribute
		{
			// --- XSPA core subject attributes ---
			public const string SubjectId = "urn:oasis:names:tc:xspa:1.0:subject:subject-id";
			public const string Organization = "urn:oasis:names:tc:xspa:1.0:subject:organization";
			public const string OrganizationId = "urn:oasis:names:tc:xspa:1.0:subject:organization-id";
			public const string ChildOrganization = "urn:oasis:names:tc:xspa:1.0:subject:child-organization";
			public const string Role = "urn:oasis:names:tc:xspa:1.0:subject:role";
			public const string PurposeOfUse = "urn:oasis:names:tc:xspa:1.0:subject:purposeOfUse";
			public const string PurposeOfUse_Helsenorge = "urn:oasis:names:tc:xspa:1.0:subject:purposeofuse";
			public const string Npi = "urn:oasis:names:tc:xspa:2.0:subject:npi";

			// --- IHE / XUA / XCA / BPPC ---
			public const string HomeCommunityIdXca = "urn:ihe:iti:xca:2010:homeCommunityId";
			public const string ProviderIdentifier = "urn:ihe:iti:xua:2017:subject:provider-identifier";
			public const string BppcDocId = "urn:ihe:iti:bppc:2007:docid";
			public const string XuaAcp = "urn:ihe:iti:xua:2012:acp";

			// --- XACML attributes ---
			public const string ResourceId10 = "urn:oasis:names:tc:xacml:1.0:resource:resource-id";
			public const string ResourceId20 = "urn:oasis:names:tc:xacml:2.0:resource:resource-id";
			public const string SubjectRole20 = "urn:oasis:names:tc:xacml:2.0:subject:role";
			public const string ActionPurpose20 = "urn:oasis:names:tc:xacml:2.0:action:purpose";

			// --- eHelse-specific attributes ---
			public const string EhelseHomeCommunityId = "urn:no:ehelse:saml:1.0:subject:homeCommunityId";
			public const string EhelseSecurityLevel = "urn:no:ehelse:saml:1.0:subject:SecurityLevel";
			public const string EhelseScope = "urn:no:ehelse:saml:1.0:subject:Scope";
			public const string EhelseClientId = "urn:no:ehelse:saml:1.0:subject:client_id";
			public const string EhelseAuthenticationMethod = "urn:no:ehelse:saml:1.0:subject:Authentication_method";
			public const string EhelseHealthcareService = "urn:no:ehelse:saml:1.1:subject:healthcareservice";

			// --- NHN Trust Framework extensions ---
			public const string TrustChildOrgName = "urn:nhn:trust-framework:1.0:ext:subject:child-organization-name";
			public const string TrustResourceChildOrg = "urn:nhn:trust-framework:1.0:ext:resource:child-organization";
			public const string TrustResourceChildOrgId = "urn:nhn:trust-framework:1.0:ext:resource:child-organization-id";
			public const string TrustHealthcareService = "urn:nhn:trust-framework:1.0:ext:care-relationship:healthcare-service";
			public const string TrustPurposeOfUseDetails = "urn:nhn:trust-framework:1.0:ext:care-relationship:purpose-of-use-details";
			public const string TrustDecisionRef = "urn:nhn:trust-framework:1.0:ext:care-relationship:decision-ref";

			// --- Generic / misc ---
			public const string SamlSubjectId = "urn:oasis:names:tc:SAML:attribute:subject-id";
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

	public static class XmlDefaultOptions
	{
		public static readonly XmlWriterSettings DefaultXmlWriterSettings = new()
		{
			Indent = true,
			Encoding = Encoding.UTF8,
			OmitXmlDeclaration = true,
		};

		public static readonly XmlWriterSettings DefaultXmlWriterSettingsInline = new()
		{
			Encoding = Encoding.UTF8,
			OmitXmlDeclaration = true,
		};
	}

	public static class Xds
	{
		public static class Namespaces
		{
			public const string Xdsb = "urn:ihe:iti:xds-b:2007";
			public const string Query = "urn:oasis:names:tc:ebxml-regrep:xsd:query:3.0";
			public const string Rs = "urn:oasis:names:tc:ebxml-regrep:xsd:rs:3.0";
			public const string Rim = "urn:oasis:names:tc:ebxml-regrep:xsd:rim:3.0";
			public const string Lcm = "urn:oasis:names:tc:ebxml-regrep:xsd:lcm:3.0";
			public const string Svs = "urn:ihe:iti:svs:2008";
			public const string Hl7V3 = "urn:hl7-org:v3";
			public const string Rmd = "urn:ihe:iti:rmd:2017";
		}

		public static class OperationContract
		{
			public const string Iti18Action = "urn:ihe:iti:2007:RegistryStoredQuery";
			public const string Iti18Reply = "urn:ihe:iti:2007:RegistryStoredQueryResponse";
			public const string Iti43Action = "urn:ihe:iti:2007:RetrieveDocumentSet";
			public const string Iti43Reply = "urn:ihe:iti:2007:RetrieveDocumentSetResponse";
			public const string Iti41Action = "urn:ihe:iti:2007:ProvideAndRegisterDocumentSet-b";
			public const string Iti41Reply = "urn:ihe:iti:2007:ProvideAndRegisterDocumentSet-bResponse";
			public const string Iti44Action = "urn:hl7-org:v3:PRPA_IN201301UV02";
			public const string Iti45Action = "urn:hl7-org:v3:PRPA_IN201309UV02";
			public const string Iti45Reply = "urn:hl7-org:v3:PRPA_IN201310UV02";
			public const string Iti48Action = "urn:ihe:iti:2008:RetrieveValueSet";
			public const string Iti48Reply = "urn:ihe:iti:2008:RetrieveValueSetResponse";
			public const string Iti60Action = "urn:ihe:iti:2010:RetrieveMultipleValueSets";
			public const string Iti60Reply = "urn:ihe:iti:2010:RetrieveMultipleValueSetsResponse";
			public const string Iti62Action = "urn:ihe:iti:2010:DeleteDocumentSet";
			public const string Iti62Reply = "urn:ihe:iti:2010:DeleteDocumentSetResponse";
			public const string Iti86Action = "urn:ihe:iti:2017:RemoveDocuments";
			public const string Iti86Reply = "urn:ihe:iti:2017:RemoveDocumentsResponse";
		}

		public static class StoredQueries
		{
			public const string FindDocuments = "urn:uuid:14d4debf-8f97-4251-9a74-a90016b0af0d";
			public const string GetAll = "urn:uuid:10b545ea-725c-446d-9b95-8aeb444eddf3";
			public const string GetDocuments = "urn:uuid:5c4f972b-d56b-40ac-a5fc-c8ca9b40b9d4";

			//Not supported by DIPS
			public const string FindSubmissionSets = "urn:uuid:f26abbcb-ac74-4422-8a30-edb644bbc1a9";
			public const string FindFolders = "urn:uuid:958f3006-baad-4929-a4de-ff1114824431";
			public const string GetFolders = "urn:uuid:5737b14c-8a1a-4539-b659-e03a34a5e1e4";
			public const string GetAssociations = "urn:uuid:a7ae438b-4bc2-4642-93e9-be891f7bb155";
			public const string GetDocumentsAndAssociations = "urn:uuid:bab9529a-4a10-40b3-a01f-f68a615d247a";
			public const string GetSubmissionSets = "urn:uuid:51224314-5390-4169-9b91-b1980040715a";
			public const string GetSubmissionSetAndContents = "urn:uuid:e8e3cb2c-e39c-46b9-99e4-c12f57260b83";
			public const string GetFolderAndContents = "urn:uuid:b909a503-523d-4517-8acf-8e5834dfc4c7";
			public const string GetFoldersForDocument = "urn:uuid:10cae35a-c7f9-4cf5-b61e-fc3278ffb578";
			public const string GetRelatedDocuments = "urn:uuid:d90e5407-b356-4d91-a89f-873917b4b0e6";
			public const string FindDocumentsByReferenceId = "urn:uuid:12941a89-e02e-4be5-967c-ce4bfc8fe492";
		}

		public static class QueryParamters
		{
			public static class DocumentEntry
			{
				public const string PatientId = "$XDSDocumentEntryPatientId";
				public const string ClassCode = "$XDSDocumentEntryClassCode";
				public const string TypeCode = "$XDSDocumentEntryTypeCode";
				public const string PracticeSettingCode = "$XDSDocumentEntryPracticeSettingCode";
				public const string CreationTimeFrom = "$XDSDocumentEntryCreationTimeFrom";
				public const string CreationTimeTo = "$XDSDocumentEntryCreationTimeTo";
				public const string ServiceStartTimeFrom = "$XDSDocumentEntryServiceStartTimeFrom";
				public const string ServiceStartTimeTo = "$XDSDocumentEntryServiceStartTimeTo";
				public const string ServiceStopTimeFrom = "$XDSDocumentEntryServiceStopTimeFrom";
				public const string ServiceStopTimeTo = "$XDSDocumentEntryServiceStopTimeTo";
				public const string HealthcareFacilityTypeCode = "$XDSDocumentEntryHealthcareFacilityTypeCode";
				public const string EventCodeList = "$XDSDocumentEntryEventCodeList";
				public const string ConfidentialityCode = "$XDSDocumentEntryConfidentialityCode";
				public const string AuthorPerson = "$XDSDocumentEntryAuthorPerson";
				public const string FormatCode = "$XDSDocumentEntryFormatCode";
				public const string Status = "$XDSDocumentEntryStatus";
				public const string Type = "$XDSDocumentEntryType";
				public const string EntryUuid = "$XDSDocumentEntryEntryUUID";
				public const string UniqueId = "$XDSDocumentEntryUniqueId";
			}

			public static class Submission
			{
				public const string Status = "$XDSSubmissionSetStatus";
			}

			public static class Folder
			{
				public const string Status = "$XDSFolderStatus";
			}

			public static class General
			{
				public const string PatientId = "$patientId";
			}
		}

		public static class StatusValues
		{
			public const string Submitted = "urn:oasis:names:tc:ebxml-regrep:StatusType:Submitted";
			public const string Approved = "urn:oasis:names:tc:ebxml-regrep:StatusType:Approved";
			public const string Deprecated = "urn:oasis:names:tc:ebxml-regrep:StatusType:Deprecated";
		}

		public static class ReturnType
		{
			public const string LeafClass = "LeafClass";
			public const string ObjectRef = "ObjectRef";
		}

		public static class ResponseStatusTypes
		{
			public const string Failure = "urn:oasis:names:tc:ebxml-regrep:ResponseStatusType:Failure";
			public const string Success = "urn:oasis:names:tc:ebxml-regrep:ResponseStatusType:Success";
			public const string PartialSuccess = "urn:ihe:iti:2007:ResponseStatusType:PartialSuccess";
		}

		public static class ErrorSeverityType
		{
			public const string Warning = "urn:oasis:names:tc:ebxml-regrep:ErrorSeverityType:Warning";
			public const string Error = "urn:oasis:names:tc:ebxml-regrep:ErrorSeverityType:Error";
		}

		public static class Uuids
		{
			public static class SubmissionSet
			{
				public const string SubmissionSetClassificationNode = "urn:uuid:a54d6aa5-d40d-43f9-88c5-b4633d873bdd";
				public const string Author = "urn:uuid:a7058bb9-b4e4-4307-ba5b-e3f0ab85e12d";
				public const string ContentTypeCode = "urn:uuid:aa543740-bdda-424e-8c96-df4873be8500";
				public const string PatientId = "urn:uuid:6b5aea1a-874d-4603-a4bc-96a0a7b38446";
				public const string SourceId = "urn:uuid:554ac39e-e3fe-47fe-b233-965d2a147832";
				public const string UniqueId = "urn:uuid:96fdda7c-d067-4183-912e-bf5ee74998a8";
			}

			public static class DocumentEntry
			{
				public const string StableDocumentEntries = "urn:uuid:7edca82f-054d-47f2-a032-9b2a5b5186c1";
				public const string OnDemandDocumentEntries = "urn:uuid:34268e47-fdf5-41a6-ba33-82133c465248";
				public const string Author = "urn:uuid:93606bcf-9494-43ec-9b4e-a7748d1a838d";
				public const string ClassCode = "urn:uuid:41a5887f-8865-4c09-adf7-e362475b143a";
				public const string ConfidentialityCode = "urn:uuid:f4f85eac-e6cb-4883-b524-f2705394840f";
				public const string EventCodeList = "urn:uuid:2c6b8cb7-8b2a-4051-b291-b1ae6a575ef4";
				public const string FormatCode = "urn:uuid:a09d5840-386c-46f2-b5ad-9c3699a4309d";
				public const string HealthCareFacilityTypeCode = "urn:uuid:f33fb8ac-18af-42cc-ae0e-ed0b0bdb91e1";
				public const string PatientId = "urn:uuid:58a6f841-87b3-4a3e-92fd-a8ffeff98427";
				public const string PracticeSettingCode = "urn:uuid:cccf5598-8b07-4b77-a05e-ae952c785ead";
				public const string TypeCode = "urn:uuid:f0306f51-975f-434e-a61c-c59651d33983";
				public const string UniqueId = "urn:uuid:2e82c1f6-a085-4c72-9da3-8640a32e42ab";
				public const string ReferenceIdList = "urn:ihe:iti:xds:2013:referenceIdList";
			}

			public static class Folder
			{
				public const string FolderClassificationNode = "urn:uuid:d9d542f3-6cc4-48b6-8870-ea235fbc94c2";
				public const string CodeList = "urn:uuid:1ba97051-7806-41a8-a48b-8fce7af683c5";
				public const string PatientId = "urn:uuid:f64ffdf0-4b97-4e06-b79f-a52b38ec2f8a";
				public const string UniqueId = "urn:uuid:75df8f67-9973-4fbe-a900-df66cefecc5a";
				public const string Association = "urn:uuid:abd807a3-4432-4053-87b4-fd82c643d1f3";
			}
		}

		public static class ObjectTypes
		{
			public const string Classification = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:Classification";
			public const string Association = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:Association";
			public const string RegistryPackage = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:RegistryPackage";
			public const string ExternalIdentifier = "urn:oasis:names:tc:ebxml-regrep:ObjectType:RegistryObject:ExternalIdentifier";
		}

		public static class AssociationType
		{
			public const string HasMember = "urn:oasis:names:tc:ebxml-regrep:AssociationType:HasMember";
			public const string Replace = "urn:ihe:iti:2007:AssociationType:RPLC";
			public const string Transformation = "urn:ihe:iti:2007:AssociationType:XFRM";
			public const string Addendum = "urn:ihe:iti:2007:AssociationType:APND";
			public const string ReplaceWithTransformation = "urn:ihe:iti:2007:AssociationType:XFRM_RPLC";
			public const string DigitalSignature = "urn:ihe:iti:2007:AssociationType:signs";
			public const string SnapshotOfOnDemandDocumentEntry = "urn:ihe:iti:2010:AssociationType:IsSnapshotOf";
		}

		public static class ExternalIdentifierNames
		{
			public const string SubmissionSetPatientId = "XDSSubmissionSet.patientId";
			public const string SubmissionSetSourceId = "XDSSubmissionSet.sourceId";
			public const string SubmissionSetUniqueId = "XDSSubmissionSet.uniqueId";
			public const string DocumentEntryPatientId = "XDSDocumentEntry.patientId";
			public const string DocumentEntryUniqueId = "XDSDocumentEntry.uniqueId";
		}

		public static class ClassificationNames
		{
			public const string Author = "author";
			public const string SubmissionSetAuthor = "XDSSubmissionSet.author";
		}

		public static class SlotNames
		{
			public const string AuthorRole = "authorRole";
			public const string AuthorPerson = "authorPerson";
			public const string AuthorSpecialty = "authorSpecialty";
			public const string AuthorInstitution = "authorInstitution";
			public const string AuthorTelecommunication = "authorTelecommunication";
			public const string CreationTime = "creationTime";
			public const string LanguageCode = "languageCode";
			public const string LegalAuthenticator = "legalAuthenticator";
			public const string CodingScheme = "codingScheme";
			public const string SubmissionSetStatus = "SubmissionSetStatus";
			public const string PreviousVersion = "PreviousVersion";
			public const string SubmissionTime = "submissionTime";
			public const string IntendedRecipient = "intendedRecipient";
			public const string SourcePatientInfo = "sourcePatientInfo";
			public const string Size = "size";
			public const string Hash = "hash";
			public const string RepositoryUniqueId = "repositoryUniqueId";
			public const string ServiceStartTime = "serviceStartTime";
			public const string ServiceStopTime = "serviceStopTime";
			public const string SourcePatientId = "sourcePatientId";
		}

		public static class ErrorCodes
		{
			public const string XdsUnavailableCommunity = "XDSUnavailableCommunity";
			public const string XdsRepositoryError = "XDSRepositoryError";
			public const string XdsRegistryError = "XDSRegistryError";
			public const string XdsRepositoryBusy = "XDSRepositoryBusy";
			public const string XdsRegistryBusy = "XDSRegistryBusy";
			public const string XdsDocumentUniqueIdError = "XDSDocumentUniqueIdError";
			public const string XdsMetadataUpdateError = "XDSMetadataUpdateError";

			//Ikke en del av IHEs XDS-profil, men en norsk verdi
			public const string XdsRepresentationBelowMinimumAgeError = "XDSRepresentationBelowMinimumAgeError";
		}
	}
}