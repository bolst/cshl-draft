using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace CSHLDraft.Data;

/// <summary>
/// Creates a link between <see cref="Supabase"/> and <see cref="AuthenticatedState"/> to provide support for using
/// Gotrue with Blazor's built in Authentication handler.
/// https://github.com/acupofjose/supasharp-todo/blob/master/SupasharpTodo.Shared/Providers/SupabaseAuthenticationStateProvider.cs
/// </summary>
public class CustomAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly Supabase.Client _supabase;
    private readonly CustomUserService _customUserService;

    private AuthenticationState AnonymousState => new(new ClaimsPrincipal(new ClaimsIdentity()));

    public CustomAuthenticationStateProvider(Supabase.Client supabase, CustomUserService customUserService)
    {
        // Console.WriteLine($"{nameof(CustomAuthenticationStateProvider)} initialized.");
        _supabase = supabase;
        _supabase.Auth.AddStateChangedListener(SupabaseAuthStateChanged);
        _customUserService = customUserService;
    }

    public void Dispose()
    {
        _supabase.Auth.RemoveStateChangedListener(SupabaseAuthStateChanged);
    }

    /// <summary>
    /// Adds a listener on the supabase client that will notify components of a change in authentication state in realtime.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="state"></param>
    private void SupabaseAuthStateChanged(IGotrueClient<User, Session> sender, Constants.AuthState state)
    {
        switch (state)
        {
            case Constants.AuthState.SignedIn:
                if (sender.CurrentUser?.Email is null)
                {
                    NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState));
                    break;
                }
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                break;
            case Constants.AuthState.SignedOut:
                NotifyAuthenticationStateChanged(Task.FromResult(AnonymousState));
                break;
        }
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // check if supabase has a current user
        if (_supabase.Auth.CurrentUser?.Email is not null)
        {
            var currentUser = await _customUserService.LookupAccountAsync(_supabase.Auth.CurrentUser.Email);
            return currentUser is null ? AnonymousState : new AuthenticationState(currentUser.ToClaimsPrincipal());
        }
        
        // otherwise try to refresh the session
        var (access, refresh) = await _customUserService.FetchPersistedTokensAsync();
        if (string.IsNullOrEmpty(access) || string.IsNullOrEmpty(refresh))
        {
            // Console.WriteLine("No session found, returning as anonymous.");
            return AnonymousState;
        }

        try
        {
            await _supabase.Auth.SetSession(access, refresh);
        }
        catch { }

        if (_supabase.Auth.CurrentUser?.Email == null)
        {
            // Console.WriteLine("An authenticated user not found, returning as anonymous.");
            return AnonymousState;
        }

        var account = await _customUserService.LookupAccountAsync(_supabase.Auth.CurrentUser.Email);
        return account is null ? AnonymousState : new AuthenticationState(account.ToClaimsPrincipal());
    }

    public async Task<string> LoginAsync(LoginFormModel form)
    {
        string errorMessage = string.Empty;
        
        try
        {
            var session = await _supabase.Auth.SignIn(form.Email, form.Password);
            if (session is not null)
            {
                await _customUserService.PersistSessionAsync(session);
            }
            else
            {
                errorMessage = "Invalid credentials";
            }
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException e)
        {
            errorMessage = "Invalid credentials";
        }
        catch (Exception e)
        {
            errorMessage = "Something went wrong...";
        }

        return errorMessage;
    }

    public async Task LogoutAsync()
    {
        await _customUserService.ClearBrowserStorageAsync();
        await _supabase.Auth.SignOut();
    }
}