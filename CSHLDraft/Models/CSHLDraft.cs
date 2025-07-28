namespace CSHLDraft.Data;

public class CSHLDraft
{
    public Guid Id { get; set; }
    public bool Snake { get; set; }
    public string State { get; set; }
    public string Name { get; set; }
    public DateTime? DTStart { get; set; }
    public int creator_account_id { get; set; }
    public string StrGmAccountIds { get; set; }

    private IEnumerable<int>? _gmAccountIds;
    public IEnumerable<int> GmAccountIds
    {
        get
        {
            _gmAccountIds ??= StrGmAccountIds.Split(';').Select(int.Parse);
            return _gmAccountIds;
        }
    }


    public TimeSpan? TimeStart
    {
        get => DTStart?.TimeOfDay;
        set
        {
            DTStart ??= new();
            DTStart = DTStart.Value.Date + value;
        }
    }
}