using NorskHelsenettUpload;

namespace PatientHealthRecordsEPJ.UploadDocuments.Lists; 
	
public class TypeCodeList
{
	public static List<SimpleCoding> TypeCodes = new List<SimpleCoding>()
	{
		  new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "A01-2", Display = "Kriseplan" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "A02-2", Display = "Individuell plan" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "A03-2", Display = "Epikrise" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "A04-2", Display = "Sykepleiesammenfatning" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "A05-2", Display = "Fysioterapisammenfatning" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "A06-2", Display = "Ergoterapisammenfatning" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "A07-2", Display = "Psykologsammenfatning" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "A08-2", Display = "Sosionomsammenfatning" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "A09-2", Display = "Ernæringsfysiologsammenfatning" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "A10-2", Display = "Annet fagpersonell sammenfatning" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "A11-2", Display = "Tverrfaglig sammenfatning" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "A12-2", Display = "Utskrivings-/Pasientorientering" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "A13-2", Display = "Poliklinisk epikrise" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "B01-2", Display = "Tverrfaglig behandlingsplan" }, // ⚠️ your Razor had mismatched value (B02-2)
        new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "B02-2", Display = "Journalnotat" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "B03-2", Display = "Poliklinisk notat" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "C01-2", Display = "Medisinsk biokjemi" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "C02-2", Display = "Blodbank og immunologi" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "C03-2", Display = "Mikrobiologi, virologi og serologi" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "C04-2", Display = "Patologi, histologi og cytologi" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "C05-2", Display = "Klinisk farmakologi" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "C06-2", Display = "Medisinsk genetikk" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "C07-2", Display = "Allergiutredning" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "D01-2", Display = "Hjerte og kretsløp" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "D02-2", Display = "Lunge" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "D03-2", Display = "Fordøyelse" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "D04-2", Display = "Urinveier" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "D05-2", Display = "Gyn/Reproduksjon" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "D06-2", Display = "Nervesystemet" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "D07-2", Display = "Ledd/ ben/ skjelett" }, // ⚠️ HTML typo: had extra "t"
        new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "D08-2", Display = "ØNH" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "D09-2", Display = "Øye" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "D10-2", Display = "Hud" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "D11-2", Display = "Endokrinologi" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "D12-2", Display = "Metabolisme" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "D13-2", Display = "Beinmargsutstryk" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "E01-2", Display = "Bildediagnostiske svar" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "E02-2", Display = "Foto og film" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "F01-2", Display = "Kurve" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "F02-2", Display = "Anestesi- og opr. rapporter" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "F03-2", Display = "Intensiv/postoperativ observasjon" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "F04-2", Display = "Svangerskap og fødsel" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "F05-2", Display = "Diabetes/ endokrinologi" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "F06-2", Display = "Onkologi/ hematologi" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "F07-2", Display = "Nyre/ dialyse" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "F08-2", Display = "Smertebehandling" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "F09-2", Display = "Ambulansejournal" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "F10-2", Display = "Transplantasjon" },
		new SimpleCoding() { System = "2.16.840.1.113883.4.642.3.240", Code = "11524-6", Display = "EKG" }, // loinc.org code
        new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "I01-2", Display = "Henvisninger" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "I02-2", Display = "Brev" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "J01-2", Display = "Sykmeldinger og trygdesaker" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "J02-2", Display = "Legeerklæring om dødsfall" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "S01-2", Display = "Tester" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "S02-2", Display = "Systematiserte diagnostiske intervju" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9602", Code = "S03-2", Display = "Voldsrisikovurdering" }
	}; 
}
