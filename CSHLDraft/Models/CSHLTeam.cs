namespace CSHLDraft.Data;


public class CSHLTeam
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string LogoUrl { get; set; }
    public int draft_id { get; set; }
    public int Pick { get; set; }
}