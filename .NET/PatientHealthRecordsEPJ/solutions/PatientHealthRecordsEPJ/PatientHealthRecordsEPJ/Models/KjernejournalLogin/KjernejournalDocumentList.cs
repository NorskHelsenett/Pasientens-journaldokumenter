namespace PatientHealthRecordsEPJ.Models.HelsenorgeLogin;

//{"erSperret":false,"erBlokkert":false}
//{ "erOperator":false,"pasientFodselsdato":"1955-05-17"}


public class KjernejournalDocumentList
{
	public int prefix1 { get; set; }
	public int prefix2 { get; set; }
	public int prefix3 { get; set; }
	public int prefix4 { get; set; }
	public int prefix5 { get; set; }
	public int prefix6 { get; set; }
	//public Errorlist[] errorList { get; set; }
	public List<Errorlist> errorList { get; set; }
	public int blockedDocuments { get; set; }
	public int filtrerteDokumenterPaTidsbegrensning { get; set; }
	//public Journaldocument[] journalDocuments { get; set; }
	public List<Journaldocument> journalDocuments { get; set; }
	public bool harSignertBruksvilkar { get; set; }

	public static KjernejournalDocumentList CreateRandomDocumentList(int count)
	{
		KjernejournalDocumentList list = new KjernejournalDocumentList()
		{
			journalDocuments = new List<Journaldocument>()
		}; 

		for (int i = 0; i < count; i++)
		{
			Journaldocument document = new Journaldocument()
			{
				documentName = "Document " + (i + 1),
				createdDateTime = DateTime.Now,
				documentUniqueId = Guid.NewGuid().ToString(),
			}; 

			list.journalDocuments.Add(document);
		}
		
		return list; 
	}	
}

public class Errorlist
{
	public string errorCode { get; set; }
	public string errorLocation { get; set; }
	public string errorMessage { get; set; }
}

public class Journaldocument
{
	public DateTime createdDateTime { get; set; }
	public string documentType { get; set; }
	public string documentName { get; set; }
	public string institutionName { get; set; }
	public string institutionDepartmentName { get; set; }
	public object procedure { get; set; }
	public string authorName { get; set; }
	public object authorRole { get; set; }
	public string documentStatus { get; set; }
	public string homeCommunityId { get; set; }
	public string repositoryUniqueId { get; set; }
	public string classCode { get; set; }
	public string typeCode { get; set; }
	public string typeName { get; set; }
	public string documentUniqueId { get; set; }
	public string localId { get; set; }
	public object[] confidentialityCode { get; set; }
}
