using NorskHelsenettUpload.Ihe.Mhd.Models;

namespace NorskHelsenettUpload.Ihe.Mhd.Interfaces; 

public interface IBundleCreator
{    
	string CreateFhirBundle(ProvideDocumentBundleRequest request, string Type, bool removeBinaryDataForDisplay = false);
}
