using Microsoft.AspNetCore.SignalR;

namespace CSHLDraft.Hubs;

public class DraftHub : Hub
{
    public const string HubUrl = "/drafthub";

    public async Task PushDraftUpdate()
    {
        await Clients.Others.SendAsync(Events.OnDraftUpdate);
    }

    public async Task PushDraftStateChange()
    {
        await Clients.Others.SendAsync(Events.OnDraftStateChange);
    }

    public async Task PushTimerStateChange(string jsCommand)
    {
        await Clients.Others.SendAsync(Events.OnTimerStateChange, jsCommand);
    }

    public record Events
    {
        public const string OnDraftUpdate = "OnDraftUpdate";
        public const string OnDraftStateChange = "OnDraftStateChange";
        public const string OnTimerStateChange = "OnTimerStateChange";
    }
}