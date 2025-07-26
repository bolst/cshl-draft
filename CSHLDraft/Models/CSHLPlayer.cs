namespace CSHLDraft.Data;


public class CSHLPlayer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public string Height { get; set; }
    public string Weight { get; set; }
    public string HeadshotUrl { get; set; }
    public int draft_id { get; set; }
}