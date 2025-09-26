#if false
using NorskHelsenettUpload.Ihe.Mhd.Interfaces;
using NorskHelsenettUpload.Ihe.Mhd.Models;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;
using NorskHelsenettUpload.Models;
using static System.Net.WebRequestMethods;
//using RtfPipe.Tokens;
using System.Numerics;

namespace NorskHelsenettUpload.Ihe.Mhd.Services; 

public class BundleCreator : IBundleCreator
{
    /// <summary>
    /// Creates FHIR Bundle according to IHE MHD-specification for Comprehensive structure (v 4.2.1)
    /// https://profiles.ihe.net/ITI/MHD/ITI-65.html
    /// 
    /// Comprehensive Metadata: https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.Comprehensive.ProvideBundle
    /// - this is otherwise known as XDS-on-FHIR
    /// - shall be a Transaction Bundle
    /// - all resources shall be compliant with comprehensive constraints, they may be marked comprehensive
    /// - shall create a SubmissionSet type List that is either minimal, comprehensive, or unContained
    /// - may create one or more DocumentReference that is either minimal, comprehensive, or unContained
    ///     - with a document as a Binary
    ///     - or, when implementing the ITI-65 FHIR Document Publish option, a FHIR Document Bundle
    /// - may create/update one or more Folder type List that is either minimal, comprehensive, or unContained
    /// - may create/update/read one Patient
    /// </summary>
    /// <param name="model"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    //public string CreateFhirBundle(XdsRequest model, string type)

    public string CreateFhirBundle(ProvideDocumentBundleRequest model, string type)
	{
        // TODO: implemented just Comprehensive type of Bundle ... "Minimal" and "UnContained.Comprehensive.Full"

        //*** Patient Object ***//

        var patientGuidId = Guid.NewGuid().ToString();
        var binaryGuidId = Guid.NewGuid().ToString();

        // Define ID-type
        string identifierType = "";
        switch (model.DocumentWithEntry.Metadata!.SourcePatientInfo!.NinType)
        {
            case "Dnumber": identifierType = "urn:oid:2.16.578.1.12.4.1.4.2"; break;
            case "NationalIdentityNumber": identifierType = "urn:oid:2.16.578.1.12.4.1.4.1"; break;
        }

        // FHIR Patient-object
        var reqPatient = new Hl7.Fhir.Model.Patient
        {
            Id = patientGuidId,
            Identifier = [
                new Identifier{
                Value = model.Submission.PatientId,
                System = identifierType
            }
            ],
            Text = new Narrative
            {
                Status = Narrative.NarrativeStatus.Extensions,
                Div = "<div xmlns=\"http://www.w3.org/1999/xhtml\">SubmissionSet with Patient</div>"
            }
        };

        // Patient's Name => SourcePatientInfo
        if (!string.IsNullOrEmpty(model.DocumentWithEntry.Metadata!.SourcePatientInfo!.Name.FamilyName))
        {
            // Family Name
            var patientName = new HumanName
            {
                Family = model.DocumentWithEntry.Metadata!.SourcePatientInfo!.Name.FamilyName
            };


            // First Name
            if (!string.IsNullOrEmpty(model.DocumentWithEntry.Metadata!.SourcePatientInfo!.Name.FirstName))
            {
                patientName.Given = [model.DocumentWithEntry.Metadata!.SourcePatientInfo!.Name.FirstName!];
            }

            // Middle Name - Extension
            if (!string.IsNullOrEmpty(model.DocumentWithEntry.Metadata.SourcePatientInfo.Name.MiddleName))
            {
                patientName.Extension =
                [
                    new Extension
                    {
                        Url = "http://hl7.no/fhir/StructureDefinition/no-basis-middlename",
                        Value = new FhirString(model.DocumentWithEntry.Metadata.SourcePatientInfo.Name.MiddleName)
                    }
                ];
            }

            // Add final name to the Patient-object
            reqPatient.Name = [patientName];

        }

        // Patient's BirthDate
        if (model.DocumentWithEntry.Metadata.SourcePatientInfo.DateOfBirth != default)
        {
            reqPatient.BirthDate = model.DocumentWithEntry.Metadata!.SourcePatientInfo!.DateOfBirth!.ToString("yyyy'-'MM'-'dd");
        }

        // Patient's Gender
        if (!string.IsNullOrEmpty(model.DocumentWithEntry.Metadata.SourcePatientInfo.Gender))
        {
            reqPatient.Gender = model.DocumentWithEntry.Metadata.SourcePatientInfo.Gender switch
            {
                "M" => AdministrativeGender.Male,
                "F" => AdministrativeGender.Female,
                "O" => AdministrativeGender.Other,
                "U" => AdministrativeGender.Unknown,
                _ => AdministrativeGender.Unknown
            };
        }

        // Tuple for Authors
        var listAuthors = new List<(Practitioner author, List<Organization> org, PractitionerRole practRole)> ();

        // Document Author(s) - object
        foreach (var author in model.DocumentWithEntry.Metadata!.Authors)
        {
            // Author - FHIR Practitioner-object
            var reqDocAuthor = new Practitioner
            {
                Id = Guid.NewGuid().ToString(),
            };
            var temp_name = new HumanName();

            if (author.Name != null)
            {
                if (!string.IsNullOrEmpty(author.Name.FamilyName))
                {
                    temp_name.Family = author.Name.FamilyName;
                }

                if (!string.IsNullOrEmpty(author.Name.GivenName))
                {
                    temp_name.Given = [author!.Name.GivenName];//.Append<string>(author.Name.FamilyName);
                }

                if (!string.IsNullOrEmpty(author.Name.MiddleName))
                {
                    var listGivenName = temp_name.Given.ToList();
                    listGivenName.Add(author!.Name.MiddleName);
                    temp_name.Given = listGivenName;
                }
            }
            
            reqDocAuthor.Name.Add(temp_name);

            // Author - FHIR Organization-object
            var listAuthorInstitution = new List<Organization>();  
            foreach (var inst in author.InstitutionName!)
            {
                var reqDocAuthorOrganization = new Organization
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = inst.Name,
                };

                if (!string.IsNullOrEmpty(inst.Identifier))
                {
                    reqDocAuthorOrganization.Identifier = 
                        [ 
                            new Identifier
                            {
                                Value = inst.Identifier,
                                System = "urn:oid:" + inst.AssigningAuthority,
                            }
                        ];
                }
                listAuthorInstitution.Add(reqDocAuthorOrganization);
            }

            // Author - FHIR PractitionerRole-object
            var reqDocAuthorRole = new PractitionerRole
            {
                Id = Guid.NewGuid().ToString(),
                Code = []
            };

            // Add all author's roles in the list in current PractitionerRole
            foreach (var role in author.Role!)
            {
                reqDocAuthorRole.Code.Add(new CodeableConcept
                {
                    Coding = [
                        new Coding { Display = role }
                        ]
                });
            };

            // Add auhtor's specialty to current PractitionerRole
            if (author.Specialty != null)
            {
                reqDocAuthorRole.Specialty =
                [
                    new CodeableConcept {
                        Coding = [
                        new Coding { Display = author.Specialty }
                        ]
                    }
                ];
            }
            reqDocAuthorRole.Practitioner = new ResourceReference()
            {
                Reference = "#" + reqDocAuthor.Id
            };
            reqDocAuthorRole.Organization = new ResourceReference()
            {
                Reference = "#" + listAuthorInstitution.First().Id
            };

            // Add Tuple to the list (author, list of org.names + role and specialty in PractitionerRole
            listAuthors.Add((reqDocAuthor, listAuthorInstitution, reqDocAuthorRole)); 
        }

        // Legal Authenticator
        List<Practitioner> listLegalAuthenticator = [];
        if (model.DocumentWithEntry.Metadata.LegalAuthenticator.ToHl7String() != "^^^^^^") {
            var reqDocLegalAuthenticator = new Practitioner
            {
                Id = Guid.NewGuid().ToString(),
            };
            
            var temp_name = new HumanName();
            if (!string.IsNullOrEmpty(model.DocumentWithEntry.Metadata.LegalAuthenticator.FamilyName))
            {
                temp_name.Family = model.DocumentWithEntry.Metadata.LegalAuthenticator.FamilyName;
            }

            if (!string.IsNullOrEmpty(model.DocumentWithEntry.Metadata.LegalAuthenticator.GivenName))
            {
                temp_name.Given = [model.DocumentWithEntry.Metadata.LegalAuthenticator.GivenName];
            }

            if (!string.IsNullOrEmpty(model.DocumentWithEntry.Metadata.LegalAuthenticator.MiddleName))
            {
                var listGivenName = temp_name.Given.ToList();
                listGivenName.Add(model.DocumentWithEntry.Metadata.LegalAuthenticator.MiddleName);
                temp_name.Given = listGivenName;
            }

            reqDocLegalAuthenticator.Name.Add(temp_name);

            listLegalAuthenticator.Add(reqDocLegalAuthenticator);
        }

        //*** Documents Binary added to the Entry ***//
        var reqDocuments = new Bundle.EntryComponent
        {
            FullUrl = "urn:uuid:" + binaryGuidId,
            Resource = new Binary
            {
                Id = binaryGuidId,
                Meta = new ()
                {
                    Profile = ["http://hl7.org/fhir/StructureDefinition/Binary"]
                },
                Data = model.DocumentWithEntry.Content,
                ContentType = model.DocumentWithEntry.MimeType
            },
            Request = new Bundle.RequestComponent
            {
                Method = Bundle.HTTPVerb.POST,
                Url = "Binary"
            },
        };

        //*** DocumentReference Entry ***//
        var codingClassCode = model.DocumentWithEntry.Metadata.ClassCode.Split('|');
        var codingTypeCode = model.DocumentWithEntry.Metadata.TypeCode.Split('|');
        var codingHealthCareFacilityType = model.DocumentWithEntry.Metadata.HealthCareFacilityType.Split('|');
        var codingPracticeSettingCode = model.DocumentWithEntry.Metadata.PracticeSettingCode.Split('|');

        var reqDocReference = new DocumentReference
        {
            Id = model.DocumentWithEntry.Metadata.UniqueId.ToString(),
            MasterIdentifier = new Identifier
            {
                Use = Identifier.IdentifierUse.Usual, // entryUUID	
                System = "urn:ietf:rfc:3986",
                Value = "urn:uuid:" + model.DocumentWithEntry.Metadata.UniqueId.ToString()
            },
            Status = DocumentReferenceStatus.Current,
            Type = new CodeableConcept
            {
                Coding = [
                    new Coding
                    {
                        System = "urn:oid:" + codingTypeCode[0],
                        Code = codingTypeCode[1],
                        Display = (!string.IsNullOrEmpty(codingTypeCode[2]))? codingTypeCode[2]: "Missing display name for typeCode"
                    }
                ]
            },
            Category =
            [
                new CodeableConcept
                {
                    Coding = [
                        new Coding {
                            System = "urn:oid:" + codingClassCode[0],
                            Code = codingClassCode[1],
                            Display = (!string.IsNullOrEmpty(codingClassCode[2])) ? codingClassCode[2] : "Missing display name for classCode"
                        }
                    ]
                }
            ],
            Subject = new ResourceReference
            {
                Reference = "Patient/" + patientGuidId
            },
            Date = model.DocumentWithEntry.CreationTime,
            Author = [],
            SecurityLabel = [],
            Content = [
                new DocumentReference.ContentComponent {
                    Attachment =  new Attachment
                    {
                        ContentType = model.DocumentWithEntry.MimeType,
                        Language = model.DocumentWithEntry.LanguageCode,
                        Size = model.DocumentWithEntry.Size,
                        Hash = model.DocumentWithEntry.Hash,
                        Title = model.DocumentWithEntry.Title,
                        Url = reqDocuments.FullUrl,
                        Creation = model.DocumentWithEntry.CreationTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ")
                    },
                    Format = new Coding {
                        System = "http://ihe.net/fhir/ihe.formatcode.fhir/CodeSystem/formatcode",
                        Code = model.DocumentWithEntry.Metadata.FormatCode
                    }
                }
            ],
            Context = new DocumentReference.ContextComponent
            {
                Period = new Period
                {
                    Start = model.DocumentWithEntry.Metadata.ServiceStartTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ"),
                    End = model.DocumentWithEntry.Metadata.ServiceStopTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ")
                },
                FacilityType = new CodeableConcept
                {
                    Coding =
                    {
                        new Coding
                        {
                            System = "urn:oid:" + codingHealthCareFacilityType[0],
                            Code = codingHealthCareFacilityType[1],
                            Display = (!string.IsNullOrEmpty(codingHealthCareFacilityType[2]))?codingHealthCareFacilityType[2]:"Missing display name for healthcare facility type"
                        }
                    }
                },
                PracticeSetting = new CodeableConcept
                {
                    Coding =
                    {
                        new Coding
                        {
                            System = "urn:oid:" + codingPracticeSettingCode[0],
                            Code = codingPracticeSettingCode[1],
                            Display = (!string.IsNullOrEmpty(codingPracticeSettingCode[2]))? codingPracticeSettingCode[2]: "Missing displayName for practice setting code"
                        }
                    }
                },
                SourcePatientInfo = new ResourceReference  // Refering to patient object which SHOULD contain patient's name, gender and birthday
                {
                    Reference = "#" + patientGuidId // model.DocumentWithEntry.Metadata.PatientId
                }
            },
            Meta = new Meta
            {
                Profile = ["https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.Comprehensive.DocumentReference"],
                Security = []
                    
            },
            Contained = [reqPatient],
            Description = model.DocumentWithEntry.Metadata.Comment,
            Text = new Narrative()
            {
                Div = "<div xmlns=\"http://www.w3.org/1999/xhtml\">Narrative for DocumentRefs</div>",
                Status = Narrative.NarrativeStatus.Generated
            },
        };

        // ConfidentialityCode-List
        if (model.DocumentWithEntry.Metadata.ConfidentialityCodeList.Count > 0)
        {
            foreach (var confidentialityCode in model.DocumentWithEntry.Metadata.ConfidentialityCodeList)
            {
                var confidentiality = confidentialityCode.Split('|');
                var confCode = new Coding
                {
                    System = "urn:oid:" + confidentiality[0],
                    Code = confidentiality[1],
                    Display = (!string.IsNullOrEmpty(confidentiality[2])) ? confidentiality[2] : "Missing display name for confidentiality code"
                };

                reqDocReference.Meta.Security.Add(confCode);

                reqDocReference.SecurityLabel.Add(
                        new CodeableConcept
                        {
                            Coding = [
                            new Coding{
                                System = "urn:oid:" + confCode.System,
                                Code = confCode.Code,
                                Display = confCode.Display
                            }]
                        }
                    );
            }
        }

        // EventCode-List
        if (model.DocumentWithEntry.Metadata.EventCodeList!.Count > 0)
        {
            foreach (var eventCode in model.DocumentWithEntry.Metadata.EventCodeList)
            {
                var evCode = eventCode.Split('|');

                reqDocReference.Context.Event.Add(new()
                {
                    Coding = {
                        new Coding
                        {
                            System = "urn:oid:" + evCode[0],
                            Code = evCode[1],
                            Display = (!string.IsNullOrEmpty(evCode[2])) ? evCode[2] : "Missing display name for event code"
                        }
                    }
                });
            }
        }

        // IHE: 3.65.4.1.2.3 Replace, Transform, Signs, and Append Associations
        // The DocumentReference.relatesTo element indicates an association between DocumentReference resources.
        if (model.Association.Type != "urn:oasis:names:tc:ebxml-regrep:AssociationType:HasMember")
        {
            DocumentRelationshipType method = model.Association.Type switch
            {
                "urn:ihe:iti:2007:AssociationType:APND" => DocumentRelationshipType.Appends,
                "urn:ihe:iti:2007:AssociationType:XFRM" => DocumentRelationshipType.Transforms,
                "urn:ihe:iti:2007:AssociationType:signs" => DocumentRelationshipType.Signs,
                "urn:ihe:iti:2007:AssociationType:RPLC" => DocumentRelationshipType.Replaces,
                _ => throw new Exception("DocumentRelationship - operation not supported"),
            };

            reqDocReference.RelatesTo = 
            [
                new() {
                    Code = method,
                    Target = new ResourceReference
                    {
                        Reference = "reference_to_another_document[not_implemented]" // Should refer to document instance which this rule applies to
                    }
                }
            ];
        }

        // Add contained Author-resources to DocumentReference
        foreach (var author in listAuthors)
        {
            reqDocReference.Contained.Add(author.author);
            reqDocReference.Contained.Add(author.practRole);
            foreach (var org in author.org)
            {
                reqDocReference.Contained.Add(org);
            }
        }

        // Add Author-references to DocumentReference
        foreach (var author in listAuthors)
        {
            reqDocReference.Author.Add(new ResourceReference
            {
                Reference = "#" + author.author.Id,
                Type = "Practitioner"
            });
            reqDocReference.Author.Add(new ResourceReference
            {
                Reference = "#" + author.practRole.Id,
                Type = "PractitionerRole"
            });
            foreach (var org in author.org)
            {
                reqDocReference.Author.Add(new ResourceReference
                {
                    Reference = "#" + org.Id,
                    Type = "Organization"
                });
            }
        }

        // Add LegalAuthenticator to DocumentReference
        if (listLegalAuthenticator.Count != 0)
        {
            reqDocReference.Authenticator = new ();
            foreach (var authenticator in listLegalAuthenticator)
            {
                // Create reference to Contained-resource
                reqDocReference.Authenticator = new ResourceReference
                {
                    Reference = "#" + authenticator.Id
                };

                // Create Contained-resource for each LegalAuthenticator-resource
                reqDocReference.Contained.Add(authenticator);
            }
        }

        var reqDocumentRefs = new Bundle.EntryComponent
        {
            FullUrl = "urn:uuid:" + model.DocumentWithEntry.Metadata.UniqueId.ToString(),
            Resource = reqDocReference,
            Request = new Bundle.RequestComponent
            {
                Method = Bundle.HTTPVerb.POST,
                Url = $"DocumentReference/{model.DocumentWithEntry.Metadata.UniqueId}"
            },
        };

        //*** SubmissionSet Entry ***//

        // Submission Author
        var reqDocSubmAuthor = new Practitioner
        {
            Id = Guid.NewGuid().ToString(),
        };

        var name = new HumanName();

        if (model.Submission.Author!.FirstOrDefault()!.Name != null)
        {
            if (!string.IsNullOrEmpty(model.Submission.Author!.FirstOrDefault()!.Name!.FamilyName))
            {
                name.Family = model.Submission.Author!.FirstOrDefault()!.Name!.FamilyName;
            }

            if (!string.IsNullOrEmpty(model.Submission.Author!.FirstOrDefault()!.Name!.GivenName))
            {
                name.Given = [model.Submission.Author!.FirstOrDefault()!.Name!.GivenName];
            }

            if (!string.IsNullOrEmpty(model.Submission.Author!.FirstOrDefault()!.Name!.MiddleName))
            {
                var listGivenName = name.Given.ToList();
                listGivenName.Add(model.Submission.Author!.FirstOrDefault()!.Name!.MiddleName);
                name.Given = listGivenName;
            }
        }
        reqDocSubmAuthor.Name.Add(name);


        // Submission object
        var oidUniqueId = $"2.25." + new BigInteger(model.Submission.UniqueId.ToByteArray()).ToString().Trim('-');//.ToString("N0");

        var reqList = new Hl7.Fhir.Model.List
        {
            Id = model.Submission.UniqueId.ToString(),
            Meta = new Meta
            {
                Profile = ["https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.Comprehensive.SubmissionSet"]
            },
            Extension =
            [
                new () // extension (designationType) => SubmissionSet.contentTypeCode
                {
                    Url = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-designationType",
                    Value = new CodeableConcept
                    {
                        Coding =  [
                                new Coding {
                                    System = "urn:oid:" + codingTypeCode[0],
                                    Code = codingTypeCode[1],
                                    Display = (!string.IsNullOrEmpty(codingTypeCode[2])) ? codingTypeCode[2] : "Missing display name for type code"
                                }
                            ]
                    }
                },
                new() {
                        Url = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-sourceId",
                        Value = new Identifier
                        {
                            Value = "urn:oid:" + model.Submission.SourceId.ToString()
                        }
                    }
            ],
            Identifier = [
                new() {
                    Use = Identifier.IdentifierUse.Usual, // SubmissionSetUniqueIdIdentifier
                    System="urn:ietf:rfc:3986",
                    Value = "urn:oid:" + oidUniqueId // must start with "urn:oid:"
                    
                },
                new() {
                    Use = Identifier.IdentifierUse.Official, // EntryUUIDIdentifier
                    System="urn:ietf:rfc:3986",
                    Value = "urn:uuid:" + model.DocumentWithEntry.Metadata.UniqueId.ToString()
                }
            ],
            Contained = [reqDocSubmAuthor],
            Mode = ListMode.Working, // Documentation: The processing mode that applies to this list. (???) - working | snapshot | changes
            
            Code = new CodeableConcept
            {
                Coding = [
                  new Coding {
                        System = "https://profiles.ihe.net/ITI/MHD/CodeSystem/MHDlistTypes",
                        Code = "submissionset"
                   }
                ]
            },
            Subject = new ResourceReference
            {
                Reference = "Patient/" + patientGuidId //+ model.Submission.PatientId
            },
            Date = model.Submission.SubmissionTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ"),
            Source = new ResourceReference
            {
                Reference = "#" + reqDocSubmAuthor.Id,
            },
            Title = model.Submission.Title,
            Text = new Narrative() 
            {
                Div = "<div xmlns=\"http://www.w3.org/1999/xhtml\">SubmissionSet Narrative</div>",
                Status = Narrative.NarrativeStatus.Generated
            },
            Entry = [new List.EntryComponent { Item = new ResourceReference { Reference = "urn:uuid:" + reqDocReference.Id } }]
        };

        // Submission Author Organization (in case it's not empty)
        if (model.Submission.Author!.FirstOrDefault()!.InstitutionName!.Length > 0)
        {
            var authOrg = model.Submission.Author!.FirstOrDefault()!.InstitutionName;
            var reqDocSubmAuthorOrganization = new Organization
            {
                Id = Guid.NewGuid().ToString(),
                Name = authOrg!.FirstOrDefault()!.Name!
            };

            if (!string.IsNullOrEmpty(authOrg?.FirstOrDefault()!.Identifier))
            {
                reqDocSubmAuthorOrganization.Identifier = [ new Identifier
                {
                    Value = authOrg?.FirstOrDefault()!.Identifier,
                    System = "urn:oid:" + authOrg?.FirstOrDefault()!.AssigningAuthority,
                }];
            }
            // Add contained object
            reqList.Contained.Add(reqDocSubmAuthorOrganization);
            // Add reference to contained object
            reqList.Source.Extension.Add(
                    new()
                    {
                        Url = "https://profiles.ihe.net/ITI/MHD/StructureDefinition/ihe-authorOrg",
                        Value = new ResourceReference
                        {
                            Reference = "#" + reqDocSubmAuthorOrganization.Id
                        }
                    }
                );
        }

        // Submission Comment
        if (!string.IsNullOrEmpty(model.Submission.Comments))
        {
            reqList.Note = [new Annotation { Text = model.Submission.Comments }];
        }

        // Submission Type
        if (model.Submission.AvailabilityStatus != null)
        { 
            switch (model.Submission.AvailabilityStatus)
            {
                case "urn:oasis:names:tc:ebxml-regrep:StatusType:Approved": reqList.Status = List.ListStatus.Current; break;
                case "urn:oasis:names:tc:ebxml-regrep:StatusType:Deprecated": reqList.Status = List.ListStatus.Retired; break;
            }

        };

        // Submission Entry
        var reqDocSubmission = new Bundle.EntryComponent
        {
            FullUrl = "urn:uuid:" + reqList.Id,
            Resource = reqList,
            Request = new Bundle.RequestComponent
            {
                Method = Bundle.HTTPVerb.POST,
                Url = $"List/{reqList.Id}",
            },
        };

        // Final bundle
        var reqBundle = new Bundle
        {
            Id = Guid.NewGuid().ToString(),
            Type = Bundle.BundleType.Transaction,
            Timestamp = DateTime.Now,
            Meta = new Meta
            {
                Profile = ["https://profiles.ihe.net/ITI/MHD/StructureDefinition/IHE.MHD.Comprehensive.ProvideBundle"],
                Security = [
                        new Coding
                    {
                        System = "http://terminology.hl7.org/CodeSystem/v3-ActReason",
                        Code = "HTEST"
                    }
                    ]
            },
            Entry = { reqDocSubmission, reqDocumentRefs, reqDocuments }
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new DateTimeConverter()
            }
        }
        .ForFhir(typeof(Bundle).Assembly)
        .Pretty();

        return _ = JsonSerializer.Serialize(reqBundle, options);
    }

    // Serializing options
    public class DateTimeConverter : JsonConverter<System.DateTime>
    {
        public override System.DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(System.DateTime));
            return System.DateTime.Parse(reader.GetString()!);
        }

        public override void Write(Utf8JsonWriter writer, System.DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssZ", CultureInfo.InvariantCulture));
        }
    }

    // ******************************************* MOCK Functions **************************************************** /






   


    private static byte[] GetByteArray(int sizeInKb)
    {
        Random rnd = new();
        byte[] b = new byte[sizeInKb * 1024];
        rnd.NextBytes(b);
        return b;
    }

}

#endif