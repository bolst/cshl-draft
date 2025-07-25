using Microsoft.AspNetCore.SignalR.Client;

namespace CSHLDraft.Data;


public interface ICSHLData
{
    Task<CSHLAccount?> GetAccountByEmailAsync(string email);
    Task UpdateRefreshTokenAsync(CSHLRefreshToken refreshToken);
    Task<CSHLRefreshToken?> GetRefreshTokenAsync(Guid localId, string provider);
    
    
    Task<IEnumerable<CSHLDraftPick>> GetDraftPicksAsync(int draftId); 
    Task<CSHLDraftPick?> GetMostRecentDraftPickAsync(int draftId);
    Task<IEnumerable<CSHLTeam>> GetTeamsInDraftAsync(int draftId);
    Task<CSHLTeam?> GetTeamWithCurrentPickAsync(int draftId);
    Task DraftPlayerAsync(int draftId, CSHLPlayer player, CSHLTeam team, CSHLDraftPick pick);
    Task ResetDraftAsync(int draftId);
}


public class CSHLData(string connectionString) : DapperBase(connectionString), ICSHLData
{
    
    public async Task<CSHLAccount?> GetAccountByEmailAsync(string email)
    {
        string sql = @"select * from account where email = @Email limit 1";

        return await QueryDbSingleAsync<CSHLAccount>(sql, new { Email = email });
    }        
        
    public async Task UpdateRefreshTokenAsync(CSHLRefreshToken refreshToken)
    {
        string sql = @"INSERT INTO auth_token(refresh, local_id, provider, access)
                        VALUES (@refresh, @local_id, @provider, @access)";
        
        await ExecuteSqlAsync(sql, refreshToken);
    }        
    
    public async Task<CSHLRefreshToken?> GetRefreshTokenAsync(Guid localId, string provider)
    {
        string sql = @"SELECT *
                        FROM auth_token
                        WHERE local_id = @local_id
                            AND provider = @provider
                        ORDER BY created_at DESC
                        LIMIT 1";
        
        return await QueryDbSingleAsync<CSHLRefreshToken>(sql, new { LocalId = localId, Provider = provider });
    }        
    
    
    public async Task<IEnumerable<CSHLDraftPick>> GetDraftPicksAsync(int draftId)
    {
        return [];
    }
    
    public async Task<CSHLDraftPick?> GetMostRecentDraftPickAsync(int draftId)
    {
        return null;
    }
    
    public async Task<IEnumerable<CSHLTeam>> GetTeamsInDraftAsync(int draftId)
    {
        return [];
    }
    
    public async Task<CSHLTeam?> GetTeamWithCurrentPickAsync(int draftId)
    {
        return null;
    }
    
    public async Task DraftPlayerAsync(int draftId, CSHLPlayer player, CSHLTeam team, CSHLDraftPick pick)
    {
        
    }
    
    public async Task ResetDraftAsync(int draftId)
    {
        
    }
    
}