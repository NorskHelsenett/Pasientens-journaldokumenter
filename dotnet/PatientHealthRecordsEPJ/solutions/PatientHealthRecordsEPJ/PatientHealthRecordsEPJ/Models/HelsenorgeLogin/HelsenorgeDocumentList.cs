namespace PatientHealthRecordsEPJ.Models.HelsenorgeLogin; 

public class HelsenorgeDocumentList
{
	public List<HelsenorgeDokument> Dokumenter { get; set; }

	public static HelsenorgeDocumentList CreateRandomDocumentList(int count)
	{
		HelsenorgeDocumentList list = new HelsenorgeDocumentList()
		{
			Dokumenter = new List<HelsenorgeDokument>()
		};

		for (int i = 0; i < count; i++)
		{
			HelsenorgeDokument document = new HelsenorgeDokument()
			{
				Id = Guid.NewGuid().ToString(),
				Tittel = "Document " + (i + 1),
				OpprettetDato = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
				SorterbarOpprettetDato = DateTime.Now,
				HomeId = "",
				RepoId = "",				
			};

			list.Dokumenter.Add(document);
		}

		return list;
	}
}

public class HelsenorgeDokument
{
	public string Id { get; set; }
	public string HomeId { get; set; }
	public string RepoId { get; set; }
	public string Tittel { get; set; }
	public string OpprettetDato { get; set; }
	public DateTime SorterbarOpprettetDato { get; set; }
	public string[] Forfatter { get; set; }
	public string Organisasjon { get; set; }
	public object Avdeling { get; set; }
	public string MimeType { get; set; }
	public string DokumentType { get; set; }
	public int KategoriId { get; set; }
	public string Kategori { get; set; }
	public int FilStorrelse { get; set; }
	public bool GyldigFilStorrelse { get; set; }
	public bool ErStottet { get; set; }
	public bool ErBegrenset { get; set; }
	public string KonfidensialitetsKode { get; set; }
	public string Kilde { get; set; }
	public string UnderkategoriId { get; set; }
}
