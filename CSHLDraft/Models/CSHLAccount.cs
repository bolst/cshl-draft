using System.Security.Claims;

namespace CSHLDraft.Data;

public class CSHLAccount
{
    public int id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Roles { get; set; }
    
    public string FullName => $"{FirstName} {LastName}";

    public ClaimsPrincipal ToClaimsPrincipal()
    {
        IEnumerable<Claim> claims =
        [
            new(ClaimTypes.PrimarySid, id.ToString()),
            new(ClaimTypes.Name, FullName),
            new(ClaimTypes.Email, Email)
        ];

        foreach (var role in Roles.Split(';', StringSplitOptions.RemoveEmptyEntries))
            claims = claims.Append(new Claim(ClaimTypes.Role, role));

        return new(new ClaimsIdentity(claims, "CSHLDraft"));
    }
}