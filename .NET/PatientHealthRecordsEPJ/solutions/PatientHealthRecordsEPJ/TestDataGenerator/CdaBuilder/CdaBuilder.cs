using System;
using System.Xml.Linq;
using XcaXds.Commons.Models.Custom.RegistryDtos;

namespace XcaXds.Commons.Cda
{
	public static class CdaBuilder
	{
		private static readonly XNamespace ns = "urn:hl7-org:v3";
		private static readonly XNamespace voc = "urn:hl7-org:v3/voc";

		public static XDocument FromDocumentEntryDto(DocumentEntryDto dto)
		{
			if (dto == null)
				throw new ArgumentNullException(nameof(dto));

			string? authorName = null; 

			var author = dto.Author?.FirstOrDefault(); 

			if (author != null)
			{
				authorName = author.Person?.FirstName;

				if (author.Person?.LastName != null)
				{
					authorName += " " + author.Person?.LastName;
				}
			}
			else
			{
				authorName = "Unknown author"; 
			}


			// Prepare helper variables
			string effectiveTime = dto.CreationTime?.ToString("yyyyMMddHHmmss") ?? DateTime.UtcNow.ToString("yyyyMMddHHmmss");
			string idRoot = "2.16.578.1.12.4.3.1.1.20.2"; // Example Norwegian OID — adjust if needed
			string templateRoot = "2.16.578.1.34.10.123";  // Example template ID

			var clinicalDocument =
				new XElement(ns + "ClinicalDocument",
					new XAttribute("classCode", "DOCCLIN"),
					new XAttribute("moodCode", "EVN"),
					new XAttribute(XNamespace.Xmlns + "voc", voc),
					new XElement(ns + "realmCode", new XAttribute("code", "NO")),
					new XElement(ns + "typeId",
						new XAttribute("extension", "POCD_HD000040"),
						new XAttribute("root", "2.16.840.1.113883.1.3")),
					new XElement(ns + "templateId",
						new XAttribute("root", templateRoot)),
					new XElement(ns + "id",
						new XAttribute("extension", dto.UniqueId ?? dto.Id),
						new XAttribute("root", idRoot)),

					// document type
					new XElement(ns + "code",
						new XAttribute("code", dto.TypeCode?.Code ?? "UNK"),
						new XAttribute("displayName", dto.TypeCode?.DisplayName ?? "Unknown"),
						new XAttribute("codeSystem", dto.TypeCode?.CodeSystem ?? "2.16.840.1.113883.6.1")),

					new XElement(ns + "title", dto.Title ?? "Untitled Document"),
					new XElement(ns + "effectiveTime", new XAttribute("value", effectiveTime)),

					// patient
					dto.SourcePatientInfo != null ?
					new XElement(ns + "recordTarget",
						new XElement(ns + "patientRole",
							new XElement(ns + "id",
								new XAttribute("extension", dto.SourcePatientInfo.PatientId?.Id ?? "UNKNOWN"),
								new XAttribute("root", "2.16.578.1.12.4.1.4.1"))))
					: null,

					// author(s)
					dto.Author != null ?
					new XElement(ns + "author",
						new XElement(ns + "time", new XAttribute("value", effectiveTime)),
						new XElement(ns + "assignedAuthor",
							new XElement(ns + "id", new XAttribute("root", Guid.NewGuid())),
							new XElement(ns + "assignedPerson",
								//new XElement(ns + "name", dto.Author.FirstOrDefault()?.AuthorPerson ?? "Unknown Author"))))
						//new XElement(ns + "name", (dto.Author?.FirstOrDefault()?.Person?.FirstName ?? "Unknown Author")))))7
						new XElement(ns + "name", authorName))))
					: null,

					// custodian
					new XElement(ns + "custodian",
						new XElement(ns + "assignedCustodian",
							new XElement(ns + "representedCustodianOrganization",
								new XElement(ns + "id", new XAttribute("root", "2.16.578.1.34.100")),
								new XElement(ns + "name", "XcaDS Repository")))),

					// optional: structuredBody stub
					new XElement(ns + "component",
						new XElement(ns + "structuredBody",
							new XElement(ns + "component",
								new XElement(ns + "section",
									new XElement(ns + "title", dto.Title ?? "Section"),
									new XElement(ns + "text", "Automatically generated test document.")))))

				);

			return new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), clinicalDocument);
		}
	}
}
