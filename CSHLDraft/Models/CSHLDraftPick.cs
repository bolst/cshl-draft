namespace CSHLDraft.Data;


public class CSHLDraftPick
{
    public int Id { get; set; }
    public Guid draft_id { get; set; }
    public Guid player_id { get; set; }
    public Guid team_id { get; set; }
    public int pick { get; set; }

    public string TeamLogo { get; set; }
    public string TeamName { get; set; }
    public int round { get; set; }
    public string Name { get; set; }
    public string HeadshotUrl { get; set; }
    public string PrimaryColorHex { get; set; }
    public string SecondaryColorHex { get; set; }
    public string TertiaryColorHex { get; set; }
}