using Dapper;
using Microsoft.AspNetCore.SignalR.Client;

namespace CSHLDraft.Data;


public interface ICSHLData
{
    Task<CSHLAccount?> GetAccountByEmailAsync(string email);
    Task<IEnumerable<CSHLAccount>> SearchAccountsAsync(string name, CancellationToken token = default);
    Task UpdateRefreshTokenAsync(CSHLRefreshToken refreshToken);
    Task<CSHLRefreshToken?> GetRefreshTokenAsync(Guid localId, string provider);


    
    Task<IEnumerable<CSHLDraft>> GetDraftsAsync();
    Task<CSHLDraft?> GetDraftByIdAsync(Guid draftId);
    Task<CSHLDraft?> CreateDraftAsync(CSHLDraft draft, int accountId);
    Task UpdateDraftAsync(CSHLDraft draft);
    Task DeleteDraftAsync(Guid draftId);
    
    
    
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
    Task<CSHLDraftPickDetail?> GetPlayerDraftPickAsync(Guid draftId, Guid playerId);
    Task DraftPlayerAsync(CSHLPlayer player, CSHLDraftPickDetail pick);
    Task ResetDraftAsync(Guid draftId);
    Task UpdateDraftStateAsync(Guid draftId, string state);
    


    Task UpdateTeamLogoAsync(Guid teamId, string logoUrl);
    Task UpdatePlayerHeadshotAsync(Guid playerId, string logoUrl);
    Task UpdateTeamGmAsync(Guid teamId, CSHLAccount gm);
}


public class CSHLData(string connectionString) : DapperBase(connectionString), ICSHLData
{
    
    public async Task<CSHLAccount?> GetAccountByEmailAsync(string email)
    {
        string sql = @"select * from account where email = @Email limit 1";

        return await QueryDbSingleAsync<CSHLAccount>(sql, new { Email = email });
    }

    public async Task<IEnumerable<CSHLAccount>> SearchAccountsAsync(string name, CancellationToken token = default)
    {
        string sql = @"select * from account where lower(firstname || ' ' || lastname) like '%' || lower(@Name) || '%'";
        return await QueryDbAsync<CSHLAccount>(sql, new { Name = name }, cancellationToken: token);
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
        return await QueryDbAsync<CSHLDraft>(@"select
                                                  d.*,
                                                  string_agg(t.gmaccountid::text, ';') as strgmaccountids
                                                from
                                                  draft d
                                                  left outer join team t on t.draft_id = d.id
                                                group by
                                                  d.id");
    }

    public async Task<CSHLDraft?> GetDraftByIdAsync(Guid draftId)
    {
        return await QueryDbSingleAsync<CSHLDraft>(@"select
                                                          d.*,
                                                          string_agg(t.gmaccountid::text, ';') as strgmaccountids
                                                        from
                                                          draft d
                                                          left outer join team t on t.draft_id = d.id
                                                        where d.id = @DraftId
                                                        group by
                                                          d.id", new { DraftId = draftId });
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
    
    public async Task UpdateDraftAsync(CSHLDraft draft)
    {
        await ExecuteSqlAsync(@"UPDATE draft SET
                                            state = @State,
                                            snake = @Snake,
                                            dtstart = @DTStart
                                        WHERE id = @Id", draft);
    }

    public async Task DeleteDraftAsync(Guid draftId)
    {
        await ExecuteTransactionAsync(async () =>
        {
            var parameters = new { DraftId = draftId };
            await ExecuteSqlAsync("DELETE FROM draft_pick WHERE draft_id = @DraftId", parameters);
            await ExecuteSqlAsync("DELETE FROM player WHERE draft_id = @DraftId", parameters);
            await ExecuteSqlAsync("DELETE FROM team WHERE draft_id = @DraftId", parameters);
            await ExecuteSqlAsync(@"DELETE FROM draft WHERE id = @DraftId", parameters);
        });
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
        return await QueryDbAsync<CSHLTeam>("select * from team where draft_id = @DraftId order by pick desc", new { DraftId = draftId });
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
        return await QueryDbAsync<CSHLDraftPickDetail>("select * from draftpickdetail where draft_id = @DraftId order by pick", new { DraftId = draftId });
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
	                        t.gmaccountid,
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
	                        LEFT OUTER JOIN account a ON t.gmaccountid = a.id
                        WHERE
	                        t.id = get_team_with_current_pick (@DraftId)
                        LIMIT
	                        1";

        return await QueryDbSingleAsync<CSHLDraftPickDetail>(sql, new { DraftId = draftId });
    }

    public async Task<CSHLDraftPickDetail?> GetPlayerDraftPickAsync(Guid draftId, Guid playerId)
    {
        return await QueryDbSingleAsync<CSHLDraftPickDetail>("select * from draftpickdetail where draft_id = @DraftId and player_id = @PlayerId limit 1", new { DraftId = draftId, PlayerId = playerId });
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



    public async Task UpdateTeamLogoAsync(Guid teamId, string logoUrl)
    {
        await ExecuteSqlAsync("update team set logourl = @LogoUrl where id = @TeamId", new { TeamId = teamId, LogoUrl = logoUrl });
    }

    public async Task UpdatePlayerHeadshotAsync(Guid playerId, string logoUrl)
    {
        await ExecuteSqlAsync("update player set headshoturl = @LogoUrl where id = @PlayerId", new { PlayerId = playerId, LogoUrl = logoUrl });
    }

    public async Task UpdateTeamGmAsync(Guid teamId, CSHLAccount gm)
    {
        await ExecuteSqlAsync("update team set gmaccountid = @GmId, gmname = @GmName where id = @TeamId", new
        {
            GmId = gm.id,
            GmName = gm.FirstName + " " + gm.LastName,
            TeamId = teamId
        });
    }
}