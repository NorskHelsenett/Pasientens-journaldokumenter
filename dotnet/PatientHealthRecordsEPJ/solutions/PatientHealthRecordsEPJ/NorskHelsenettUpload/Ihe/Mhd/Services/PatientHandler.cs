using NorskHelsenettUpload.Ihe.Mhd.Interfaces;
using NorskHelsenettUpload.Ihe.Mhd.Models;
//using Microsoft.AspNetCore.Components;

namespace PatientHealthRecordsEPJ.UploadDocuments.Ihe.Mhd.Services;

public class PatientHandler : ISourcePatientInfo
{
    // Patient store available for all instances
    private static readonly Dictionary<string, SourcePatientInfo> _patientSessions = [];

    public Action? OnChange { get; set; }

    public Task ClearPatientSession(string sessionIdValue)
    {
        _patientSessions.Remove(sessionIdValue);
        return Task.CompletedTask;
    }

    public Task<SourcePatientInfo> GetPatientSessionData(string sessionIdValue)
    {
        if (!_patientSessions.ContainsKey(sessionIdValue))
        {
            return null!;
        }
        return Task.FromResult(_patientSessions[sessionIdValue]);
    }

    public Task UpsertPatientSessionData(string sessionIdValue, SourcePatientInfo patientInfo)
    {
        _patientSessions[sessionIdValue] = patientInfo;
        NotifyStateChanged();
        return Task.CompletedTask;
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
