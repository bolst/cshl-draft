namespace CSHLDraft.Data;


public class CSHLDraftPick
{
    public int Id { get; set; }
    public Guid draft_id { get; set; }
    public Guid? player_id { get; set; }
    public Guid team_id { get; set; }
    public int pick { get; set; }
    public int? gm_account_id { get; set; }
}