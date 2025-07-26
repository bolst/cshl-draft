namespace CSHLDraft.Data;


public class CSHLDraftPickDetail : CSHLDraftPick
{
    public string? GmName { get; set; }
    public string TeamLogo { get; set; }
    public string TeamName { get; set; }
    public int Round { get; set; }
    public int RoundPick { get; set; }
    public string? PlayerName { get; set; }
    public string? HeadshotUrl { get; set; }
    public string PrimaryHex { get; set; }
    public string SecondaryHex { get; set; }
}