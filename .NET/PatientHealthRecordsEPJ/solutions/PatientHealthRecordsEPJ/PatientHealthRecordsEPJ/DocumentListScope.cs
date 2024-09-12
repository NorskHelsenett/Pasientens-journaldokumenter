public enum DocumentListScopeEnum
{
	// nhn:phr/mhd/read-documentreferences							
	// Krever ikke grunnlag ("hit-access-basis") som header parameter. Ingen filtrering p� dato p� dokumenttyper i henhold til Kjernejournalforskriften.
	HelsepersonellovenParagraf45 = 0,
	// nhn:phr/mhd/read-documentreferences/kjernejournalforskriften
	// Krever grunnlag ("hit-access-basis") som header parameter. Filtrering p� dato p� dokumenttyper i henhold til Kjernejournalforskriften.
	Kjernejournalforskriften = 1        
}
