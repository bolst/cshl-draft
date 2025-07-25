using Blazored.LocalStorage;

namespace CSHLDraft.Data;


public class CustomUserService
{

    private readonly ILocalStorageService _customLocalStorageProvider;
    private readonly ICSHLData _cshlData;

    public CustomUserService(ILocalStorageService customLocalStorageProvider, ICSHLData cshlData)
    {
        _customLocalStorageProvider = customLocalStorageProvider; 
        _cshlData = cshlData;
    }

    public async Task<CSHLAccount?> LookupAccountAsync(string email)
    {
        var account = await _cshlData.GetAccountByEmailAsync(email);
        return account;
    }
    
    public async Task PersistSessionAsync(Supabase.Gotrue.Session session)
    {
        try
        {
            if (string.IsNullOrEmpty(session.AccessToken) || string.IsNullOrEmpty(session.RefreshToken))
            {
                // Console.WriteLine($"Session provided no access/refresh token");
                return;
            }
            
            var localIdStr = await _customLocalStorageProvider.GetItemAsync<string>("local_id");
            // if local storage has no value or is improper guid: we generate a new one for local storage
            // use this to lookup refresh token in db
            if (localIdStr is null || !Guid.TryParse(localIdStr, out Guid localId))
            {
                localId = Guid.NewGuid();
                await _customLocalStorageProvider.SetItemAsync("local_id", localId.ToString());
            }

            await _cshlData.UpdateRefreshTokenAsync(new CSHLRefreshToken
            {
                refresh = session.RefreshToken,
                access = session.AccessToken,
                local_id = localId,
                provider = "supabase",
            });
        }
        catch (Exception e)
        {
            // Console.WriteLine($"Failed to persist user to browser with:\n\t{e}");
        }
    }

    public async Task<(string?, string?)> FetchPersistedTokensAsync()
    {
        try
        {
            var localIdStr = await _customLocalStorageProvider.GetItemAsync<string>("local_id");
            // if local storage has no value or is improper guid: we generate a new one for local storage
            // use this to lookup refresh token in db
            if (localIdStr is null || !Guid.TryParse(localIdStr, out Guid localId))
            {
                localId = Guid.NewGuid();
                await _customLocalStorageProvider.SetItemAsync("local_id", localId.ToString());
            }

            var token = await _cshlData.GetRefreshTokenAsync(localId, "supabase");

            return (token?.access, token?.refresh);
        }
        catch (System.InvalidOperationException) { }
        catch (Exception err)
        {
            // Console.WriteLine($"Failed to load session with:\n\t{err}");
        }
        return (null, null);
    }

    public async Task ClearBrowserStorageAsync()
    {
        await _customLocalStorageProvider.ClearAsync();
    }
    
}