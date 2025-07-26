using Dapper;
using Microsoft.AspNetCore.SignalR.Client;

namespace CSHLDraft.Data;


public interface ICSHLData
{
    Task<CSHLAccount?> GetAccountByEmailAsync(string email);
    Task UpdateRefreshTokenAsync(CSHLRefreshToken refreshToken);
    Task<CSHLRefreshToken?> GetRefreshTokenAsync(Guid localId, string provider);


    
    Task<IEnumerable<CSHLDraft>> GetDraftsAsync();
    Task<CSHLDraft?> CreateDraftAsync(CSHLDraft draft, int accountId);
    Task<CSHLDraft?> GetDraftByIdAsync(Guid draftId);
    Task UpdateDraftAsync(CSHLDraft draft);
    
    
    
    Task<IEnumerable<CSHLPlayer>> GetPlayersInDraftAsync(Guid draftId);
    Task<IEnumerable<CSHLPlayer>> GetDraftAvailablePlayersAsync(Guid draftId);
    Task<IEnumerable<CSHLPlayer>> GetRosterByTeamIdAsync(Guid teamId);
    Task<IEnumerable<CSHLTeam>> GetTeamsInDraftAsync(Guid draftId);
    
    
    
    Task SetDraftPlayersAsync(Guid draftId, IEnumerable<InputPlayer> players);
    Task SetDraftPlayersAsync(Guid draftId, IEnumerable<CSHLPlayer> players);
    Task SetDraftTeamsAsync(Guid draftId, IEnumerable<InputTeam> teams);
    Task SetDraftTeamsAsync(Guid draftId, IEnumerable<CSHLTeam> teams);
    
    
    
    Task<IEnumerable<CSHLDraftPickDetail>> GetDraftPicksAsync(Guid draftId); 
    Task<CSHLDraftPickDetail?> GetCurrentPickAsync(Guid draftId);
    Task DraftPlayerAsync(CSHLPlayer player, CSHLDraftPickDetail pick);
    Task ResetDraftAsync(Guid draftId);


    
    Task UpdateDraftStateAsync(Guid draftId, string state);
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

    public async Task<CSHLDraft?> CreateDraftAsync(CSHLDraft draft, int accountId)
    {
        string sql = @"INSERT INTO draft(snake, name, creator_account_id)
                                VALUES (@Snake, @Name, @AccountId)
                        RETURNING *";
        return await QueryDbSingleAsync<CSHLDraft>(sql, new
        {
            Snake = draft.Snake,
            Name = draft.Name,
            AccountId = accountId
        });
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

    public async Task<IEnumerable<CSHLPlayer>> GetDraftAvailablePlayersAsync(Guid draftId)
    {
        return await QueryDbAsync<CSHLPlayer>("select p.* from player p where p.draft_id = @DraftId and p.id not in (select player_id from draft_pick where draft_id = @DraftId)", new { DraftId = draftId });
    }
    
    public async Task<IEnumerable<CSHLPlayer>> GetRosterByTeamIdAsync(Guid teamId)
    {
        return await QueryDbAsync<CSHLPlayer>("select p.* from player p inner join draft_pick d on d.player_id = p.id and d.team_id = @TeamId", new { TeamId = teamId });
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


    
    public async Task<IEnumerable<CSHLDraftPickDetail>> GetDraftPicksAsync(Guid draftId)
    {
        return await QueryDbAsync<CSHLDraftPickDetail>("select * from draftpickdetail where draft_id = @DraftId", new { DraftId = draftId });
    }
    
    public async Task<CSHLDraftPickDetail?> GetCurrentPickAsync(Guid draftId)
    {
        string sql = @"WITH
	                        last_pick AS (
		                        SELECT
			                        COALESCE(MAX(pick), 0) AS pick
		                        FROM
			                        draft_pick
		                        WHERE
			                        draft_id = @DraftId
	                        )
                        SELECT
	                        l.pick + 1 AS pick,
	                        t.id AS team_id,
	                        t.draft_id AS draft_id,
	                        t.gm_account_id,
	                        (a.firstname || ' '::TEXT) || a.lastname AS gmname,
	                        t.logourl AS teamlogo,
	                        t.name AS teamname,
	                        get_round (
		                        @DraftId,
		                        l.pick + 1
	                        ) AS ROUND,
	                        get_round_pick (
		                        @DraftId,
		                        l.pick + 1
	                        ) AS roundpick,
	                        t.primaryhex,
	                        t.secondaryhex
                        FROM
	                        team t
	                        CROSS JOIN last_pick l
	                        LEFT OUTER JOIN account a ON t.gm_account_id = a.id
                        WHERE
	                        t.id = get_team_with_current_pick (@DraftId)
                        LIMIT
	                        1";

        return await QueryDbSingleAsync<CSHLDraftPickDetail>(sql, new { DraftId = draftId });
    }
    
    public async Task DraftPlayerAsync(CSHLPlayer player, CSHLDraftPickDetail pick)
    {
        await ExecuteSqlAsync(@"insert into draft_pick (team_id, player_id, draft_id, pick)
                                                    values (@TeamId, @PlayerId, @DraftId, @Pick)", new
        {
            TeamId = pick.team_id,
            PlayerId = player.Id,
            DraftId = pick.draft_id,
            Pick = pick.pick,
        });
    }
    
    public async Task ResetDraftAsync(Guid draftId)
    {
        await ExecuteSqlAsync("delete from draft_pick where draft_id = @DraftId", new { DraftId = draftId });
    }



    public async Task UpdateDraftStateAsync(Guid draftId, string state)
    {
        await ExecuteSqlAsync("update draft set state = @State where id = @DraftId", new { DraftId = draftId, State = state });
    }
}