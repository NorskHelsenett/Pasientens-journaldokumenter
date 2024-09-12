# Pasientens journaldokumenter

The goal of Patient Health Records API is to substitute existing SOAP-integrations, defined by IHE XDS/XCA, 
with equally functional RESTful-based APIs to provide access to clinical documents stored at different document sources 
(IHE XCA endpoints) accross the norwegian health sector. These SOAP-services er commonly used by other internal products 
(portal for citizens - helsenorge.no and secure portal for healthcare providers - kjernejournal-portal) in Norsk helsenett.

The electronic health information exchange (HIE) provided by the Patient Health Records API is based on broad usage of IHE XDS/XCA 
infrastructure in the background while this specific API in this project is using FHIR-based specification for IHE MHDS 
by implementing FHIR REST-based API service as facade to IHE XDS/XCA.

Service provided by the Patient Health Records API is secured with OAuth2 (NHN's HelseID secure token service). OAuth2-token will be further 
transformed into the SAML-token and used in XUA-process (Cross-enterprise user assertion) before sending out to other IHE XCA-endpoints.

## Architecture
Norsk helsenett is hosting the national endpoint for XCA which should be always used as "Initiating gateway" for all request from all healthcare providers. All these healthcare providers having access to this central national endpoint have signed legal contracts and agreements with Norsk helsenett about retrieving clinical information from other sources. 


## Patient Health Records API client examples

This repo contains two example projects which show how to implement clients for the **Pasientens Journaldokumenter i Kjernejournal** APIs.

Available APIs: 

1. Patient Health Records (PHR) API
   - Test environment: https://api.pjd.test.nhn.no/swagger
   - Documentation: https://utviklerportal.nhn.no/informasjonstjenester/pasientens-journaldokumenter-i-kjernejournal/dokumentdeling/dokumentdeling/docs/introductionmd/
   
### Patient Health Records EPJ:

Live version at https://epj.pjd.test.nhn.no

Demonstrates the following features

- Log on with HelseID using pushed authorization request
- Calling the PHR API with DPoP token and DPoP proof, with a client ID which has been configured in NHN's self service portal. 
- Show document list for a patient
  - Paging, sorting and filtering by using API 
- Show document
  - PDF
  - XML, labsvar, Kith, visningsfil
- Show count of restricted documents (with confidentiality code NORS)
- Show the SAML assertion (only possible in test environment)


The APIs are protected by HelseId. This requires a HelseId client definition, which can be established in NHN's self service portal.
Please contact NHN.

### NativeClients.SimpleRequestObjectsDemo:

- Log on with HelseID using pushed authorization request
- Calling the PHR API with DPoP token and DPoP proof, with a client ID which has been configured in NHN's self service portal. 
- Retreiving a document list for a patient  
- Retreiving a document  
- Retreiving a SAML assertion (only possible in test environment)
