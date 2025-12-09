namespace NorskHelsenettUpload.Ihe.Mhd.Models;

public class AssociationHasMember
{
    string id { get; set; }
    string associationType { get; set; }
    string? sourceObject { get; set; }
    string targetObject { get; set; }
    string objectType { get; set; }
}
