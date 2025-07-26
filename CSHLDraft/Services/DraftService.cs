using Microsoft.AspNetCore.SignalR.Client;

namespace CSHLDraft.Data;

public class DraftService
{
    private readonly ICSHLData _cshlData;
    
    private CSHLDraft? _draft;
    private const int PICKS_PER_ROUND = 6;

    public DraftService(ICSHLData cshlData)
    {
        _cshlData = cshlData;
    }

    public async Task<(CSHLTeam, CSHLDraftPick)> GetTeamWithCurrentPick(Guid draftId)
    {
        var currentPick = await _cshlData.GetMostRecentDraftPickAsync(draftId);

        if (currentPick is null) // this is first overall pick
        {
            var team = (await _cshlData.GetTeamWithCurrentPickAsync(draftId))!;
            
            var pick = new CSHLDraftPick
            {
                draft_id = draftId,
                team_id = team.Id,
                pick = 1,
            };
            
            return (team, pick);
        }
        else
        {
            int order = currentPick.pick >= PICKS_PER_ROUND ? 1 : currentPick.pick + 1;

            var pick = new CSHLDraftPick
            {
                draft_id = draftId,
                pick = currentPick.pick >= PICKS_PER_ROUND ? 1 : currentPick.pick + 1,
            };
            
            return (null, pick);
        }
    }

    public async Task DraftPlayerAsync(Guid draftId, CSHLPlayer player, HubConnection hub)
    {
        var (team, pick) = await GetTeamWithCurrentPick(draftId);
        // Console.WriteLine($"{team.name} is drafting {player.name}");
        
        // 1. update database to indicate the drafted player is now on the team with current pick
        await _cshlData.DraftPlayerAsync(draftId, player, team, pick);
        
        // 2. notify subscribers that a selection has been made (e.g., timer)
        await hub.SendAsync(nameof(DraftHub.PushDraftUpdate));
    }

    public async Task<IEnumerable<CSHLTeam>> GetTeamsInDraftAsync(Guid draftId)
    {
        return await _cshlData.GetTeamsInDraftAsync(draftId);
    }

    public async Task<IEnumerable<CSHLDraftPick>> GetDraftedPlayersAsync(Guid draftId)
    {
        return await _cshlData.GetDraftPicksAsync(draftId);
    }

    public async Task ResetDraftAsync(Guid draftId, HubConnection hub)
    {
        await _cshlData.ResetDraftAsync(draftId);
        
        // notify subscribers
        await hub.SendAsync(nameof(DraftHub.PushDraftUpdate));
    }
}