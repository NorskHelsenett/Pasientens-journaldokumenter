# Pasientens journaldokumenter EPJ


Publisert her: [https://epj.pjd.test.nhn.no](https://epj.pjd.test.nhn.no)

Demo EPJ for Pasientens Journaldokumenter REST API

Demonstrerer følgende funksjonalitet: 

- Pålogging med HelseID
- Hent dokumentliste
- Hent dokument
- DPoP 
- Pushed authorization request
- Bygging av attest for tillitsrammeverk, mulighet til manuell redigering for enkel testing
- PDF visning
- XML/Kith meldinger, visningsfil, vedlegg, bilder
- API paging, sortering, filtrering
- Tell antall sperrede dokumenter (v.h.a. mottatt header parameter X-SecurityLabel-Nors-Count) - eksperimentelt

Prosjektet er bygd så enkelt som mulig og har dermed lite abstraksjon og bruker enkel lagring i sesjon. 

## Utvikling av Patient Health Records API og Demo EPJ på utviklermaskin

- Lag appsettings.Development.json i dette prosjektet og pek ApiUrls til lokale endepunkter, for eksempel

```
{
    "ApiUrls": {
        "GetDocumentReferencesApiUrl": "https://localhost:8443/R4/fhir/documentreference/_search",
        "GetDocumentApiUrl": "https://localhost:8443/mhd/iti68/document"
    },    
}

```
