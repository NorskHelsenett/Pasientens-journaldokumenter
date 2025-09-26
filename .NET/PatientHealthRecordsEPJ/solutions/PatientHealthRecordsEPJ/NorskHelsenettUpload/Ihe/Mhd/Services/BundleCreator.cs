using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using NorskHelsenettUpload.Ihe.Mhd.Interfaces;
using NorskHelsenettUpload.Ihe.Mhd.Models;
using NorskHelsenettUpload.Models;
using System.Globalization;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NorskHelsenettUpload.Ihe.Mhd.Services
{
	public class BundleCreator : IBundleCreator
	{
		public string CreateFhirBundle(ProvideDocumentBundleRequest model, string type)
		{
			model.SetPatientOnAllDocuments();  // Set the SourcePatientInfo instance on all documents

			// NOTE: This implements the Comprehensive Provide Document Bundle (ITI-65).
			// Minimal and UnContained variants can be added later behind `type`.

			// ---------- 1) Create the transaction bundle ----------
			var bundle = new Bundle
			{
				Id = Guid.NewGuid().ToString(),
				Type = Bundle.BundleType.Transaction,
				Timestamp = DateTime.UtcNow,
				Meta = new Meta
				{
					Profile = ["https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.Comprehensive.ProvideBundle"],
					Security =
					[
						new Coding
						{
							System = "http://terminology.hl7.org/CodeSystem/v3-ActReason",
							Code = "HTEST"
						}
					]
				}
			};

			// ---------- 2) One Patient for the whole submission ----------
			var patientId = Guid.NewGuid().ToString();
			var patientFullUrl = $"urn:uuid:{patientId}";
			var patientEntry = BuildPatientEntry(model, patientId);
			bundle.Entry.Add(patientEntry);

			// ---------- 3) Per-document: Binary + DocumentReference ----------
			var docRefFullUrls = new List<string>();

			foreach (var doc in model.DocumentCollection.Documents)
			{
				// Binary (only if we have in-memory content; if URL is provided, we can skip Binary)
				string? binaryFullUrl = null;
				if (doc.Content is { Length: > 0 })
				{
					var binaryId = Guid.NewGuid().ToString();
					binaryFullUrl = $"urn:uuid:{binaryId}";
					var binaryEntry = BuildBinaryEntry(doc, binaryId);
					bundle.Entry.Add(binaryEntry);
				}

				// DocumentReference
				var docRefId = (doc.Metadata?.UniqueId != Guid.Empty ? doc.Metadata!.UniqueId : Guid.NewGuid()).ToString();
				var docRefFullUrl = $"urn:uuid:{docRefId}";
				var docRefEntry = BuildDocumentReferenceEntry(model, doc, docRefId, patientFullUrl, binaryFullUrl, patientEntry.Resource);
				bundle.Entry.Add(docRefEntry);
				docRefFullUrls.Add(docRefFullUrl);
			}

			// HACK 
			var firstDocument = model.DocumentCollection.Documents.First(); 

			// ---------- 4) SubmissionSet (List) with one entry per DocumentReference ----------
			var submissionListId = model.Submission.UniqueId.ToString();
			var submissionListEntry = BuildSubmissionSetEntry(model, submissionListId, patientFullUrl, docRefFullUrls, firstDocument);
			// (Optional) Put the SubmissionSet first
			bundle.Entry.Insert(0, submissionListEntry);

			// ---------- 5) Serialize ----------
			var options = new JsonSerializerOptions
			{
				WriteIndented = true,
				Converters = { new DateTimeConverter() }
			}
			.ForFhir(typeof(Bundle).Assembly)
			.Pretty();

			return JsonSerializer.Serialize(bundle, options);
		}

		// ------------------------- Helpers -------------------------

		private static Bundle.EntryComponent BuildPatientEntry(ProvideDocumentBundleRequest model, string patientId)
		{
			//var spi = model.Patient?.SourcePatientInfo;
			var spi = model.Patient;
			var identifierType = spi?.NinType switch
			{
				"Dnumber" => "urn:oid:2.16.578.1.12.4.1.4.2",
				"NationalIdentityNumber" => "urn:oid:2.16.578.1.12.4.1.4.1",
				_ => null
			};

			var patient = new Hl7.Fhir.Model.Patient
			{
				Id = patientId,
				Text = new Narrative
				{
					Status = Narrative.NarrativeStatus.Extensions,
					Div = "<div xmlns=\"http://www.w3.org/1999/xhtml\">SubmissionSet with Patient</div>"
				}
			};

			if (!string.IsNullOrEmpty(model.Submission?.PatientId) && !string.IsNullOrEmpty(identifierType))
			{
				patient.Identifier =
				[
					new Identifier
					{
						System = identifierType,
						Value = model.Submission.PatientId
					}
				];
			}

			if (spi is not null)
			{
				if (spi.Name is not null && !string.IsNullOrWhiteSpace(spi.Name.FamilyName))
				{
					var nm = new HumanName { Family = spi.Name.FamilyName };
					if (!string.IsNullOrWhiteSpace(spi.Name.FirstName))
						nm.Given = [spi.Name.FirstName];

					if (!string.IsNullOrWhiteSpace(spi.Name.MiddleName))
					{
						var g = nm.Given?.ToList() ?? new List<string>();
						g.Add(spi.Name.MiddleName);
						nm.Given = g;
						nm.Extension =
						[
							new Extension
							{
								Url = "http://hl7.no/fhir/StructureDefinition/no-basis-middlename",
								Value = new FhirString(spi.Name.MiddleName)
							}
						];
					}

					patient.Name = [nm];
				}

				if (spi.DateOfBirth != default)
					patient.BirthDate = spi.DateOfBirth.ToString("yyyy'-'MM'-'dd");

				if (!string.IsNullOrWhiteSpace(spi.Gender))
				{
					patient.Gender = spi.Gender switch
					{
						"M" => AdministrativeGender.Male,
						"F" => AdministrativeGender.Female,
						"O" => AdministrativeGender.Other,
						"U" => AdministrativeGender.Unknown,
						_ => AdministrativeGender.Unknown
					};
				}
			}

			return new Bundle.EntryComponent
			{
				FullUrl = $"urn:uuid:{patientId}",
				Resource = patient,
				Request = new Bundle.RequestComponent
				{
					Method = Bundle.HTTPVerb.POST,
					Url = "Patient"
				}
			};
		}

		private static Bundle.EntryComponent BuildBinaryEntry(Document doc, string binaryId)
		{
			return new Bundle.EntryComponent
			{
				FullUrl = $"urn:uuid:{binaryId}",
				Resource = new Binary
				{
					Id = binaryId,
					Meta = new() { Profile = ["http://hl7.org/fhir/StructureDefinition/Binary"] },
					Data = doc.Content,
					ContentType = doc.MimeType
				},
				Request = new Bundle.RequestComponent
				{
					Method = Bundle.HTTPVerb.POST,
					Url = "Binary"
				}
			};
		}

		private static Bundle.EntryComponent BuildDocumentReferenceEntry(
			ProvideDocumentBundleRequest model,
			Document doc,
			string docRefId,
			string patientFullUrl,
			string? binaryFullUrl, // null when using external URL,
			Resource patient
		)
		{
			var m = doc.Metadata!;

			// Parse codings
			var classCode = ParseCoding(m.ClassCode);
			var typeCode = ParseCoding(m.TypeCode);
			var facilityCode = ParseCoding(m.HealthCareFacilityType);
			var practiceCode = ParseCoding(m.PracticeSettingCode);

			// Attachment: either Binary (local) or external Url
			var attachment = new Attachment
			{
				ContentType = doc.MimeType,
				Language = doc.LanguageCode,
				Size = doc.Size,
				Hash = doc.Hash,
				Title = doc.Title,
				Creation = doc.CreationTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'")
			};

			if (!string.IsNullOrWhiteSpace(binaryFullUrl))
				attachment.Url = binaryFullUrl;
			else if (!string.IsNullOrWhiteSpace(doc.Url))
				attachment.Url = doc.Url;

			var docRef = new DocumentReference
			{
				Id = docRefId,
				MasterIdentifier = new Identifier
				{
					Use = Identifier.IdentifierUse.Usual,
					System = "urn:ietf:rfc:3986",
					Value = "urn:uuid:" + docRefId
				},
				Status = DocumentReferenceStatus.Current,
				Type = ToCodeableConcept(typeCode),
				Category = [ToCodeableConcept(classCode)],
				Subject = new ResourceReference { Reference = patientFullUrl },
				Date = doc.CreationTime,
				Author = [],
				SecurityLabel = [],
				Content =
				[
					new DocumentReference.ContentComponent
					{
						Attachment = attachment,
						Format = new Coding
						{
							System = "http://ihe.net/fhir/ihe.formatcode.fhir/CodeSystem/formatcode",
							Code = m.FormatCode
						}
					}
				],
				Context = new DocumentReference.ContextComponent
				{
					Period = new Period
					{
						Start = m.ServiceStartTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"),
						End = m.ServiceStopTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'")
					},
					FacilityType = ToCodeableConcept(facilityCode),
					PracticeSetting = ToCodeableConcept(practiceCode),
					// When you want SourcePatientInfo as contained demographics, use a contained Patient instead.
					SourcePatientInfo = new ResourceReference  // Refering to patient object which SHOULD contain patient's name, gender and birthday
					{
						//Reference = "#" + patientGuidId // model.DocumentWithEntry.Metadata.PatientId
						Reference = patientFullUrl
					}
				},
				Meta = new Meta
				{
					Profile =
					[
						"https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.Comprehensive.DocumentReference"
					],
					Security = []
				},
				Description = m.Comment,
				Text = new Narrative
				{
					Div = "<div xmlns=\"http://www.w3.org/1999/xhtml\">Narrative for DocumentReference</div>",
					Status = Narrative.NarrativeStatus.Generated
				}
			};

			docRef.Contained.Add(patient); 

			// Confidentiality
			if (m.ConfidentialityCodeList?.Count > 0)
			{
				foreach (var cc in m.ConfidentialityCodeList)
				{
					var code = ParseCoding(cc);
					var coding = new Coding { System = "urn:oid:" + code.system, Code = code.code, Display = code.display };
					docRef.Meta.Security.Add(coding);
					docRef.SecurityLabel.Add(new CodeableConcept { Coding = [coding] });
				}
			}

			// Event codes
			if (m.EventCodeList?.Count > 0)
			{
				foreach (var ev in m.EventCodeList)
				{
					var code = ParseCoding(ev);
					docRef.Context.Event.Add(ToCodeableConcept(code));
				}
			}

			// Authors (contained)
			var (containedAuthors, authorRefs) = BuildAuthorsContained(m.Authors);
			foreach (var r in containedAuthors) docRef.Contained.Add(r);
			docRef.Author.AddRange(authorRefs);

			// Legal authenticator (contained, optional)
			var authenticator = BuildLegalAuthenticatorContained(m.LegalAuthenticator);
			if (authenticator is not null)
			{
				docRef.Contained.Add(authenticator);
				docRef.Authenticator = new ResourceReference { Reference = "#" + authenticator.Id };
			}

			// Associations/relatesTo (single example; extend to a collection if needed)
			if (model.Association?.Type is string assocType &&
				assocType != "urn:oasis:names:tc:ebxml-regrep:AssociationType:HasMember")
			{
				var relCode = assocType switch
				{
					/*"urn:ihe:iti:2007:AssociationType:APND" => DocumentReference.DocumentRelationshipType.Appends,
					"urn:ihe:iti:2007:AssociationType:XFRM" => DocumentReference.DocumentRelationshipType.Transforms,
					"urn:ihe:iti:2007:AssociationType:signs" => DocumentReference.DocumentRelationshipType.Signs,
					"urn:ihe:iti:2007:AssociationType:RPLC" => DocumentReference.DocumentRelationshipType.Replaces,
					_ => (DocumentReference.DocumentRelationshipType?)null*/

					"urn:ihe:iti:2007:AssociationType:APND" => DocumentRelationshipType.Appends,
					"urn:ihe:iti:2007:AssociationType:XFRM" => DocumentRelationshipType.Transforms,
					"urn:ihe:iti:2007:AssociationType:signs" => DocumentRelationshipType.Signs,
					"urn:ihe:iti:2007:AssociationType:RPLC" => DocumentRelationshipType.Replaces,
					_ => (DocumentRelationshipType?)null
				};

				if (relCode is not null)
				{
					docRef.RelatesTo =
					[
						new DocumentReference.RelatesToComponent
						{
							Code = relCode.Value,
							Target = new ResourceReference
							{
                                // TODO: Resolve to another DocumentReference FullUrl in the same bundle
                                Reference = "reference_to_another_document[not_implemented]"
							}
						}
					];
				}
			}

			return new Bundle.EntryComponent
			{
				FullUrl = $"urn:uuid:{docRefId}",
				Resource = docRef,
				Request = new Bundle.RequestComponent
				{
					Method = Bundle.HTTPVerb.POST,
					Url = "DocumentReference"
				}
			};
		}

		private static (List<Resource> contained, List<ResourceReference> authorRefs)
			BuildAuthorsContained(IEnumerable<Author>? authors)
		{
			var contained = new List<Resource>();
			var refs = new List<ResourceReference>();

			if (authors is null) return (contained, refs);

			foreach (var a in authors)
			{
				var pract = new Practitioner { Id = Guid.NewGuid().ToString() };
				var hn = new HumanName();

				if (!string.IsNullOrWhiteSpace(a?.Name?.FamilyName)) hn.Family = a.Name.FamilyName;
				var given = new List<string>();
				if (!string.IsNullOrWhiteSpace(a?.Name?.GivenName)) given.Add(a.Name.GivenName!);
				if (!string.IsNullOrWhiteSpace(a?.Name?.MiddleName)) given.Add(a.Name.MiddleName!);
				if (given.Count > 0) hn.Given = given;
				if (!string.IsNullOrWhiteSpace(a?.Name?.MiddleName))
				{
					hn.Extension =
					[
						new Extension
						{
							Url = "http://hl7.no/fhir/StructureDefinition/no-basis-middlename",
							Value = new FhirString(a.Name.MiddleName)
						}
					];
				}
				pract.Name = [hn];

				// Organizations
				var orgs = new List<Organization>();
				if (a?.InstitutionName != null)
				{
					foreach (var inst in a.InstitutionName)
					{
						var org = new Organization
						{
							Id = Guid.NewGuid().ToString(),
							Name = inst.Name,
							Identifier = string.IsNullOrWhiteSpace(inst.Identifier)
								? null
								: new List<Identifier>
								  {
									  new Identifier
									  {
										  Value = inst.Identifier,
										  System = "urn:oid:" + inst.AssigningAuthority
									  }
								  }
						};
						orgs.Add(org);
					}
				}

				// PractitionerRole
				var role = new PractitionerRole
				{
					Id = Guid.NewGuid().ToString(),
					Code = []
				};
				if (a?.Role != null)
				{
					foreach (var r in a.Role)
					{
						role.Code.Add(new CodeableConcept { Coding = [new Coding { Display = r }] });
					}
				}
				if (!string.IsNullOrWhiteSpace(a?.Specialty))
				{
					role.Specialty =
					[
						new CodeableConcept { Coding = [new Coding { Display = a.Specialty }] }
					];
				}

				role.Practitioner = new ResourceReference { Reference = "#" + pract.Id };
				if (orgs.Count > 0)
					role.Organization = new ResourceReference { Reference = "#" + orgs.First().Id };

				// Add contained resources
				contained.Add(pract);
				contained.Add(role);
				contained.AddRange(orgs);

				// Author refs
				refs.Add(new ResourceReference { Reference = "#" + pract.Id, Type = "Practitioner" });
				refs.Add(new ResourceReference { Reference = "#" + role.Id, Type = "PractitionerRole" });
				foreach (var o in orgs)
					refs.Add(new ResourceReference { Reference = "#" + o.Id, Type = "Organization" });
			}

			return (contained, refs);
		}

		//private static Practitioner? BuildLegalAuthenticatorContained(PersonName? person)
		private static Practitioner? BuildLegalAuthenticatorContained(Xcn? person)
		{
			if (person is null) return null;

			// Using the sentinel "^^^^^^" pattern like in the original code — if still relevant, guard here
			//if (person.ToHl7String?.Invoke() == "^^^^^^") return null;
			if (person.Hl7String == "^^^^^^") return null;

			var p = new Practitioner { Id = Guid.NewGuid().ToString() };
			var hn = new HumanName();

			if (!string.IsNullOrWhiteSpace(person.FamilyName)) hn.Family = person.FamilyName;
			var given = new List<string>();
			if (!string.IsNullOrWhiteSpace(person.GivenName)) given.Add(person.GivenName!);
			if (!string.IsNullOrWhiteSpace(person.MiddleName)) given.Add(person.MiddleName!);
			if (given.Count > 0) hn.Given = given;

			p.Name = [hn];
			return p;
		}

		private static Bundle.EntryComponent BuildSubmissionSetEntry(
			ProvideDocumentBundleRequest model,
			string listId,
			string patientFullUrl,
			IEnumerable<string> docRefFullUrls,
			Document firstDocument // HACK! 
			)
		{
			var codingClassCode = firstDocument.Metadata.ClassCode.Split('|');
			var codingTypeCode = firstDocument.Metadata.TypeCode.Split('|');
			var codingHealthCareFacilityType = firstDocument.Metadata.HealthCareFacilityType.Split('|');
			var codingPracticeSettingCode = firstDocument.Metadata.PracticeSettingCode.Split('|');

			// Convert GUID to 2.25.<integer> OID
			var oidUnique = $"2.25.{new BigInteger(model.Submission.UniqueId.ToByteArray()).ToString().Trim('-')}";

			// Author (SubmissionSet Source)
			var subAuthor = new Practitioner { Id = Guid.NewGuid().ToString() };
			var firstAuthor = model.Submission.Author.FirstOrDefault();
			if (firstAuthor?.Name != null)
			{
				var hn = new HumanName();
				if (!string.IsNullOrWhiteSpace(firstAuthor.Name.FamilyName)) hn.Family = firstAuthor.Name.FamilyName;
				var given = new List<string>();
				if (!string.IsNullOrWhiteSpace(firstAuthor.Name.GivenName)) given.Add(firstAuthor.Name.GivenName!);
				if (!string.IsNullOrWhiteSpace(firstAuthor.Name.MiddleName)) given.Add(firstAuthor.Name.MiddleName!);
				if (given.Count > 0) hn.Given = given;
				subAuthor.Name = [hn];
			}

			var list = new Hl7.Fhir.Model.List
			{
				Id = listId,
				Meta = new Meta
				{
					Profile = ["https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.Comprehensive.SubmissionSet"]
				},
				Extension =
				[
                    // designationType (contentTypeCode)
                    new Extension
					{
						Url = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-designationType",
						Value = new CodeableConcept
						{
							Coding = [ new Coding {
										System = "urn:oid:" + codingTypeCode[0],
										Code = codingTypeCode[1],
										Display = (!string.IsNullOrEmpty(codingTypeCode[2])) ? codingTypeCode[2] : "Missing display name for type code"
									} ]
						}
					},
					new Extension
					{
						Url = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-sourceId",
						Value = new Identifier { Value = "urn:oid:" + model.Submission.SourceId }
					}
				],
				Identifier =
				[
					new Identifier
					{
						Use = Identifier.IdentifierUse.Usual, // SubmissionSetUniqueIdIdentifier
                        System = "urn:ietf:rfc:3986",
						Value = "urn:oid:" + oidUnique
					},
					new Identifier
					{
						Use = Identifier.IdentifierUse.Official, // EntryUUIDIdentifier
                        System = "urn:ietf:rfc:3986",
						Value = "urn:uuid:" + model.Submission.UniqueId.ToString()
					}
				],
				Contained = [subAuthor],
				Mode = Hl7.Fhir.Model.ListMode.Working,
				Code = new CodeableConcept
				{
					Coding =
					[
						new Coding
						{
							System = "https://profiles.ihe.net/ITI/MHD/CodeSystem/MHDlistTypes",
							Code = "submissionset"
						}
					]
				},
				Subject = new ResourceReference { Reference = patientFullUrl },
				Date = model.Submission.SubmissionTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'"),
				Source = new ResourceReference { Reference = "#" + subAuthor.Id },
				Title = model.Submission.Title,
				Text = new Narrative
				{
					Div = "<div xmlns=\"http://www.w3.org/1999/xhtml\">SubmissionSet Narrative</div>",
					Status = Narrative.NarrativeStatus.Generated
				},
				Entry = docRefFullUrls.Select(u => new Hl7.Fhir.Model.List.EntryComponent
				{
					Item = new ResourceReference { Reference = u }
				}).ToList()
			};

			// Optional: submission author organization
			var authOrg = firstAuthor?.InstitutionName?.FirstOrDefault();
			if (authOrg is not null)
			{
				var org = new Organization
				{
					Id = Guid.NewGuid().ToString(),
					Name = authOrg.Name,
					Identifier = string.IsNullOrWhiteSpace(authOrg.Identifier)
						? null
						: new List<Identifier> {
							new Identifier { Value = authOrg.Identifier, System = "urn:oid:" + authOrg.AssigningAuthority }
						  }
				};

				list.Contained.Add(org);
				list.Source.Extension ??= [];
				list.Source.Extension.Add(new Extension
				{
					Url = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-authorOrg",
					Value = new ResourceReference { Reference = "#" + org.Id }
				});
			}

			// AvailabilityStatus → List.status
			if (!string.IsNullOrWhiteSpace(model.Submission.AvailabilityStatus))
			{
				list.Status = model.Submission.AvailabilityStatus switch
				{
					"urn:oasis:names:tc:ebxml-regrep:StatusType:Approved" => Hl7.Fhir.Model.List.ListStatus.Current,
					"urn:oasis:names:tc:ebxml-regrep:StatusType:Deprecated" => Hl7.Fhir.Model.List.ListStatus.Retired,
					_ => Hl7.Fhir.Model.List.ListStatus.Current
				};
			}

			if (!string.IsNullOrWhiteSpace(model.Submission.Comments))
				list.Note = [new Annotation { Text = model.Submission.Comments }];

			return new Bundle.EntryComponent
			{
				FullUrl = $"urn:uuid:{listId}",
				Resource = list,
				Request = new Bundle.RequestComponent
				{
					Method = Bundle.HTTPVerb.POST,
					Url = "List"
				}
			};
		}

		private static (string system, string code, string? display) ParseCoding(string? pipeSeparated)
		{
			// Expects "SYSTEM|CODE|DISPLAY?" where SYSTEM is usually an OID (without "urn:oid:")
			// Returns (system, code, display)
			if (string.IsNullOrWhiteSpace(pipeSeparated)) return (string.Empty, string.Empty, null);
			var parts = pipeSeparated.Split('|');
			var system = parts.Length > 0 ? parts[0] : string.Empty;
			var code = parts.Length > 1 ? parts[1] : string.Empty;
			var display = parts.Length > 2 ? parts[2] : null;
			return (system, code, display);
		}

		private static CodeableConcept ToCodeableConcept((string system, string code, string? display) c)
		{
			var coding = new Coding
			{
				System = string.IsNullOrWhiteSpace(c.system) ? null : "urn:oid:" + c.system,
				Code = c.code,
				Display = string.IsNullOrWhiteSpace(c.display) ? null : c.display
			};
			return new CodeableConcept { Coding = [coding] };
		}

		// -------- Serialization helper identical to your original --------
		public class DateTimeConverter : JsonConverter<DateTime>
		{
			public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
				=> DateTime.Parse(reader.GetString()!);

			public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
				=> writer.WriteStringValue(value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", CultureInfo.InvariantCulture));
		}
	}
}
