using NorskHelsenettUpload;

namespace PatientHealthRecordsEPJ.UploadDocuments.Lists; 
	
public class ConfidentialityCodeList
{
	public static List<SimpleCoding> ConfidentialityCodes = new List<SimpleCoding>()
	{
		new SimpleCoding() { System = "2.16.840.1.113883.5.25", Code = "N", Display = "Normal" }, // Ingen spesielle tilgangsbegrensninger er satt
		new SimpleCoding() { System = "2.16.840.1.113883.5.25", Code = "R", Display = "Restricted" }, // 
		new SimpleCoding() { System = "2.16.840.1.113883.5.25", Code = "V", Display = "Very restricted" }, 
        new SimpleCoding() { System = "2.16.840.1.113883.5.25", Code = "L", Display = "Low" },
		new SimpleCoding() { System = "2.16.840.1.113883.5.25", Code = "U", Display = "Unrestricted" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORT", Display = "Not allowed/accessible for healthcare personell" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORN_FFL", Display = "Not visible for patient - security reasons" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORN_FORANS", Display = "Not visible for people with parenting responsibility" },
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORN_ALL", Display = "Nektet, alle" }, // Nektet pasientinnsyn i alle pasientens dokumenter. Registrert i kritisk info begrunnet i risiko for helsepersonell.
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORN_ANG", Display = "Nektet – andre grunner" }, // Nektet pasientinnsyn – kodeverket OID 9603 dekker ikke nektningsgrunnlaget – kontakte evt. Sykehuset. Kodeverket er relativt nytt og innsynsløsninger er under utvikling – Kodeverket må da derfor være under utvikling
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORN_DUP", Display = "Nektet, duplikat" }, // Nektet pasientinnsyn, Duplikat. Brukes for å flagge duplikat dokumenter, for eksempel dokumenter som blir kopiert til et HF ved virksomehetsoverdragelse.
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORN_EPO", Display = "Nektet, eget ønske" }, // Nektet pasientinnsyn av enkeltdokument. Vises ikke elektronisk etter pasientens eget ønske.	
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORN_FFH", Display = "Nektet, fare for helsepersonell" }, // Nektet pasientinnsyn - vises ikke elektronisk fordi pasient har fremsatt alvorlige trusler og fremstår aktivt med agitasjon overfor behandler. Åpner for at helsepersonell, i helt spesielle unntakssituasjoner der de føler seg truet, skal kunne beskytte seg selv ved å nekte elektronisk pasientinnsyn på enkeltdokumenter i pasientjournalen. Dermed blir ikke helsepersonellets navn like lett tilgjengelig for pasienten.
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORN_FFL", Display = "Nektet, fare for liv" }, // Nektet pasientinnsyn av enkeltdokument. Fare for liv eller alvorlig helseskade, jf. pasbrl §5-1, 2. ledd 1. alternativ.
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORN_FOR", Display = "Nektet, foreldet" }, // Virksomheten har markert dokumentet som eldre enn datoen for når pasienter fikk automatisk elektronisk innsyn i journal.
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORN_FORANS", Display = "Nektet, foreldreansvarlig" }, // Nektet innsyn for foreldre eller andre som har foreldreansvar, dersom tungtveiende hensyn til pasienten eller brukeren taler mot det, jf. Pasient- og brukerrettighetsloven § 3-4 tredje ledd. Settes av helsepersonell. Dette gjelder også for barn under 12 år og uavhengig av om barnet selv har gitt uttrykk for ønsker om at informasjon skal holdes tilbake
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORN_FPB", Display = "Nektet, forsvarlig pasientbehandling" }, // Nektet pasientinnsyn - vises ikke elektronisk av hensyn til forsvarlig pasientbehandling. Dette brukes i tilfeller dokumenter inkluderer informasjon som pasienten av ulike grunner, ikke bør få digitalt innsyn i. Blant annet kreftdiagnoser der pasienten ikke er informert ennå eller informasjon som påvirker pågående behandling på en negativ måte.
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORN_KUT", Display = "Nektet, klart utilrådelig" }, // Nektet pasientinnsyn av enkeltdokument. Klart utilrådelig av hensyn til nærstående, jf. pasbrl §5-1, 2. ledd 2. alternativ.
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORN_UNGDOM", Display = "Nektet, ungdom" }, // Nektet innsyn for ungdom 12-16, dersom det er påtrengende nødvendig for å hindre fare for liv eller alvorlig helseskade for pasienten selv. Foreldre eller andre som har foreldreansvar får tilgang til informasjon. Settes av helsepersonell.
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORS", Display = "Sperret" }, // Tilgangsbegrensning som innebærer at innbyggeren sperrer hele eller deler av opplysningene som ligger i et behandlingsrettet helseregister slik at opplysningene ikke blir tilgjengelige for enkeltpersonell eller grupper av helsepersonell
		new SimpleCoding() { System = "2.16.578.1.12.4.1.1.9603", Code = "NORU", Display = "Utsatt innsyn for innbygger" }	// Tilgangsbegrensning som gjør at utvalgte helseopplysninger gjøres tilgjengelig for innbyggeren etter fastsatt tid. Tilgangsbegrensningen kan være satt av helsepersonell, eller satt automatisk i henhold til forhåndsdefinerte regler
	}; 
}
