using Dapper;
using Microsoft.AspNetCore.SignalR.Client;

namespace CSHLDraft.Data;


public interface ICSHLData
{
    Task<CSHLAccount?> GetAccountByEmailAsync(string email);
    Task UpdateRefreshTokenAsync(CSHLRefreshToken refreshToken);
    Task<CSHLRefreshToken?> GetRefreshTokenAsync(Guid localId, string provider);


    Task<IEnumerable<CSHLDraft>> GetDraftsAsync();
    Task<CSHLDraft?> CreateDraftAsync(CSHLDraft draft);
    Task<CSHLDraft?> GetDraftByIdAsync(Guid draftId);
    Task UpdateDraftAsync(CSHLDraft draft);
    Task<IEnumerable<CSHLPlayer>> GetPlayersInDraftAsync(Guid draftId);
    Task<IEnumerable<CSHLTeam>> GetTeamsInDraftAsync(Guid draftId);
    Task SetDraftPlayersAsync(Guid draftId, IEnumerable<InputPlayer> players);
    Task SetDraftPlayersAsync(Guid draftId, IEnumerable<CSHLPlayer> players);
    Task SetDraftTeamsAsync(Guid draftId, IEnumerable<InputTeam> teams);
    Task SetDraftTeamsAsync(Guid draftId, IEnumerable<CSHLTeam> teams);
    Task<IEnumerable<CSHLDraftPick>> GetDraftPicksAsync(Guid draftId); 
    Task<CSHLDraftPick?> GetMostRecentDraftPickAsync(Guid draftId);
    Task<CSHLTeam?> GetTeamWithCurrentPickAsync(Guid draftId);
    Task DraftPlayerAsync(Guid draftId, CSHLPlayer player, CSHLTeam team, CSHLDraftPick pick);
    Task ResetDraftAsync(Guid draftId);
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
                        WHERE local_id = @LocalId
                            AND provider = @Provider
                        ORDER BY created_at DESC
                        LIMIT 1";
        
        return await QueryDbSingleAsync<CSHLRefreshToken>(sql, new { LocalId = localId, Provider = provider });
    }        
    
    public async Task<IEnumerable<CSHLDraft>> GetDraftsAsync()
    {
        return await QueryDbAsync<CSHLDraft>("select * from draft");
    }

    public async Task<CSHLDraft?> CreateDraftAsync(CSHLDraft draft)
    {
        string sql = @"INSERT INTO draft(snake, name)
                                VALUES (@Snake, @Name)
                        RETURNING *";
        return await QueryDbSingleAsync<CSHLDraft>(sql, draft);
    }

    public async Task<CSHLDraft?> GetDraftByIdAsync(Guid draftId)
    {
        return await QueryDbSingleAsync<CSHLDraft>("select * from draft where id = @DraftId", new { DraftId = draftId });
    }

    public async Task UpdateDraftAsync(CSHLDraft draft)
    {
        await ExecuteSqlAsync(@"UPDATE draft SET
                                            state = @State,
                                            snake = @Snake,
                                            dtstart = @DTStart
                                        WHERE id = @Id", draft);
    }

    public async Task<IEnumerable<CSHLPlayer>> GetPlayersInDraftAsync(Guid draftId)
    {
        return await QueryDbAsync<CSHLPlayer>("select p.* from player p where p.draft_id = @DraftId", new { DraftId = draftId });
    }

    public async Task<IEnumerable<CSHLDraftPick>> GetDraftPicksAsync(Guid draftId)
    {
        return [];
    }
    
    public async Task<CSHLDraftPick?> GetMostRecentDraftPickAsync(Guid draftId)
    {
        return null;
    }
    
    public async Task<IEnumerable<CSHLTeam>> GetTeamsInDraftAsync(Guid draftId)
    {
        return await QueryDbAsync<CSHLTeam>("select * from team where draft_id = @DraftId", new { DraftId = draftId });
    }


    public async Task SetDraftPlayersAsync(Guid draftId, IEnumerable<InputPlayer> players)
    {
        var cshlPlayers = players.Select(x => x.ToCSHLPlayer());
        await SetDraftPlayersAsync(draftId, cshlPlayers);
    }
    
    public async Task SetDraftPlayersAsync(Guid draftId, IEnumerable<CSHLPlayer> players)
    {
        await ExecuteTransactionAsync(async () =>
        {
            await ExecuteSqlAsync("delete from player where draft_id = @DraftId", new { DraftId = draftId });

            string sql = @"INSERT INTO player(name, birthday, height, weight, headshoturl, draft_id)
                        VALUES(@Name, @Birthday, @Height, @Weight, @HeadshotURL, @DraftId)";
            var parameters = players.Select(x =>
            {
                var parameter = new DynamicParameters(x);
                parameter.Add("DraftId", draftId);
                return parameter;
            });
            await ExecuteSqlAsync(sql, parameters);
        });
    }

    public async Task SetDraftTeamsAsync(Guid draftId, IEnumerable<InputTeam> teams)
    {
        var inputTeams = teams.Select(x => x.ToCSHLTeam());
        await SetDraftTeamsAsync(draftId, inputTeams);
    }

    public async Task SetDraftTeamsAsync(Guid draftId, IEnumerable<CSHLTeam> teams)
    {
        await ExecuteTransactionAsync(async () =>
        {
            await ExecuteSqlAsync("delete from team where draft_id = @DraftId", new { DraftId = draftId });

            string sql = @"INSERT INTO team(name, logourl, draft_id, pick)
                        VALUES(@Name, @LogoUrl, @DraftId, @Pick)";
            var parameters = teams.Select(x =>
            {
                var parameter = new DynamicParameters(x);
                parameter.Add("DraftId", draftId);
                return parameter;
            });
            await ExecuteSqlAsync(sql, parameters);
        });
    }


    public async Task<CSHLTeam?> GetTeamWithCurrentPickAsync(Guid draftId)
    {
        return null;
    }
    
    public async Task DraftPlayerAsync(Guid draftId, CSHLPlayer player, CSHLTeam team, CSHLDraftPick pick)
    {
        
    }
    
    public async Task ResetDraftAsync(Guid draftId)
    {
        
    }
    
}