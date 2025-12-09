using System.Security.Claims;

namespace NorskHelsenettUpload.Models;

// Add properties to this class and update the server and client AuthenticationStateProviders
// to expose more information about the authenticated user to the client.
public sealed class UserInfo
{
    public required string UserId { get; init; }
    public required string Name { get; init; }
    public string? GivenName { get; init; }
    public string? MiddleName { get; init; }
    public string? FamilyName { get; init; }
    public string? HprNumber { get; init; }
    public string? ClientId { get; set; }
    public string? ClientType { get; set; }
    public string? ClientOrgNr { get; set; }

    // Constants from JWT
    public const string UserIdClaimType = "helseid://claims/identity/pid";
    public const string NameClaimType = "name";
    public const string GivenNameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
    public const string MiddleNameClaimType = "middle_name";
    public const string FamilyNameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
    public const string HprClaimType = "helseid://claims/hpr/hpr_number";
    public const string ClientIdClaimType = "client_id";
    public const string ClientTypeClaimType = "helseid://claims/client/client_tenancy";
    public const string ClientOrgNrClaimType = "helseid://claims/client/claims/orgnr_parent";

    public static UserInfo FromClaimsPrincipal(ClaimsPrincipal principal)
    {
        return new()
        {
            UserId = GetRequiredClaim(principal, UserIdClaimType),
            Name = GetRequiredClaim(principal!, NameClaimType),
            GivenName = GetRequiredClaim(principal, GivenNameClaimType),
            MiddleName = GetRequiredClaim(principal, MiddleNameClaimType),
            FamilyName = GetRequiredClaim(principal, FamilyNameClaimType),
            HprNumber = GetRequiredClaim(principal, HprClaimType),
            //ClientId = GetRequiredClaim(principal, ClientIdClaimType),
            //ClientType = GetRequiredClaim(principal, ClientTypeClaimType),
            //ClientOrgNr = GetRequiredClaim(principal, ClientOrgNrClaimType)
        };
    }

    public ClaimsPrincipal ToClaimsPrincipal() =>
        new(new ClaimsIdentity(
            [ 
            new(UserIdClaimType, UserId), 
            new(NameClaimType, Name), 
            new(GivenNameClaimType, GivenName!),
            new(FamilyNameClaimType , FamilyName!),
            new(MiddleNameClaimType, MiddleName!),
            new(HprClaimType, HprNumber!)
            ],
            authenticationType: nameof(UserInfo),
            nameType: NameClaimType,
            roleType: null));

    private static string GetRequiredClaim(ClaimsPrincipal principal, string claimType) =>
        principal.FindFirst(claimType)?.Value ?? throw new InvalidOperationException($"Could not find required '{claimType}' claim.");

    public void UpdateClaims(List<(string,string)> claimList)
    {

    }
}
