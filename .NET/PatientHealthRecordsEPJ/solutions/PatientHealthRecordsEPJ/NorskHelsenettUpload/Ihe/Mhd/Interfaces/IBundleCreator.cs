using NorskHelsenettUpload.Ihe.Mhd.Models;

namespace NorskHelsenettUpload.Ihe.Mhd.Interfaces; 

public interface IBundleCreator
{
    //string CreateFhirBundle(XdsRequest request, string Type);
	string CreateFhirBundle(ProvideDocumentBundleRequest request, string Type);
}
