using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PatientHealthRecordsEPJ.Personverninnstillinger;

public class PersonvernDataType
{
    [JsonPropertyName("innbyggerFnr")]
    public string InnbyggerFnr { get; set; } = null!;

    [JsonPropertyName("definisjonGuid")]
    public string DefinisjonGuid { get; set; } = null!;

    [JsonPropertyName("definisjonNavn")]
    public string DefinisjonNavn { get; set; } = null!;

    [JsonPropertyName("partKode")]
    public string PartKode { get; set; } = null!;

    [JsonPropertyName("typePi")]
    public string TypePi { get; set; } = null!;

    [JsonPropertyName("status")]
    public string Status { get; set; } = null!;

    [JsonPropertyName("sekvensnummer")]
    public int Sekvensnummer { get; set; }

    [JsonPropertyName("opprettetTidspunkt")]    
    public string? OpprettetTidspunkt { get; set; }

    [JsonPropertyName("sistEndretTidspunkt")]    
    public string? SistEndretTidspunkt { get; set; }

    [JsonPropertyName("SaMetadata")]
    public SaMetadata? SaMetadata { get; set; }

    [JsonPropertyName("ReMetadata")]
    public Reservasjonmetadata ReservasjonMetadata { get; set; } = null!;

    [JsonPropertyName("TbMetadata")]
    public TbMetadata TilgangsbegrensningMetadata { get; set; } = null!;
}

public class SaMetadata
{
    [JsonPropertyName("saFasteMetadata")]
    public SaFasteMetadata SaFasteMetadata { get; set; }
}

public class SaFasteMetadata
{
    [JsonPropertyName("omfangElementer")]
    public List<Omfangelement> OmfangElementer { get; set; } = null!;
}


public class Reservasjonmetadata
{
    [JsonPropertyName("reFasteMetadata")]
    public ReFasteMetadata ReFasteMetadata { get; set; } = null!;
}

public class ReFasteMetadata
{
    [JsonPropertyName("omfangElementer")]
    public List<Omfangelement> OmfangElementer { get; set; } = null!;
}

public class TbMetadata
{
    [JsonPropertyName("tbFasteMetadata")]
    public TbFasteMetadata TbFasteMetadata { get; set; } = null!;

    [JsonPropertyName("tbInnbyggerMetadata")]
    public Tbinnbyggermetadata TbInnbyggerMetadata { get; set; } = null!;
}

public class TbFasteMetadata
{
    [JsonPropertyName("omfangElementer")]
    public List<Omfangelement> OmfangElementer { get; set; } = null!;
}

public class Omfangelement
{
    [JsonPropertyName("omfangKode")]
    public string OmfangKode { get; set; } = null!;

    [JsonPropertyName("logiskOmfang")]
    public string LogiskOmfang { get; set; } = null!;

    [JsonPropertyName("typeAngivelse")]
    public string TypeAngivelse { get; set; } = null!;
}

public class Tbinnbyggermetadata
{
    [JsonPropertyName("tidsbegrensning")]
    public Tidsbegrensning Tidsbegrensning { get; set; } = null!;

    [JsonPropertyName("detaljertAngivelse")]
    public Detaljertangivelse DetaljertAngivelse { get; set; } = null!;
}

public class Tidsbegrensning
{
    [JsonPropertyName("perioder")]    
    public List<Periode> TimePeriods { get; set; } = null!;    
}

public class Periode
{
    [JsonPropertyName("fraDato")]        
    public string FraDato { get; set; } = null!;
    
    [JsonPropertyName("tilDato")]
    public string TilDato { get; set; } = null!;     
}

public class Detaljertangivelse
{
    [JsonPropertyName("navngittHelseperson")]
    public List<Navngitthelseperson> NavngittHelsepersoner { get; set; } = null!;
}

public class Navngitthelseperson
{
    [JsonPropertyName("nummer")]
    public string HprNummer { get; set; } = null!;

    [JsonPropertyName("navn")]
    public string Navn { get; set; } = null!;
}
