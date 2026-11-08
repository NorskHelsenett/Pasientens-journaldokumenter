using NorskHelsenettUpload.Ihe.Mhd.Models;

namespace NorskHelsenettUpload.Ihe.Mhd.Interfaces; 

public interface ISourcePatientInfo
{
    Task UpsertPatientSessionData(string sessionIdValue, SourcePatientInfo patientInfo);

    Task<SourcePatientInfo> GetPatientSessionData(string sessionIdValue);

    Task ClearPatientSession(string sessionIdValue);
}
