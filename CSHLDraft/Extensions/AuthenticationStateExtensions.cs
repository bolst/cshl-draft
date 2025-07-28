using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

public static class AuthenticationStateExtensions
{
    public static string? GetEmail(this AuthenticationState state) => state.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
    
    
}