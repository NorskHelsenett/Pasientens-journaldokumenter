namespace NorskHelsenettUpload.Ihe.Mhd.Models;

public static class AssociationType
{
    

    public static readonly string Replace = "urn:ihe:iti:2007:AssociationType:RPLC";
    public static readonly string Transformation = "urn:ihe:iti:2007:AssociationType:XFRM";
    public static readonly string Addendum = "urn:ihe:iti:2007:AssociationType:APND";
    public static readonly string ReplaceWithTransformation = "urn:ihe:iti:2007:AssociationType:XFRM_RPLC";
    public static readonly string DigitalSignature = "urn:ihe:iti:2007:AssociationType:signs";
    public static readonly string SnapshotOnDemandDocumentEntry = "nurn:ihe:iti:2010:AssociationType:IsSnapshotOf";


	// public static readonly string MembershipInRegistryPackage = "urn:oasis:names:tc:ebxml-regrep:AssociationType:HasMember"; (see below for comment about HasMember) 


	/* 
        In ITI-65, membership to a submission is not expressed with HasMember URNs anymore.
        Instead, the SubmissionSet (List) directly lists its members. 

        {
          "resourceType": "List",
          "id": "submission-set-1",
          "status": "current",
          "mode": "working",
          "entry": [
            { "item": { "reference": "DocumentReference/doc1" } },
            { "item": { "reference": "DocumentReference/doc2" } }
          ]
        }


        That entry array is the FHIR equivalent of the HasMember association.
    
        The SubmissionSet (List) resource already plays that role by containing references to the DocumentReferences it groups.

    */


}
