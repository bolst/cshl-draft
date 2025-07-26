namespace CSHLDraft.Data;

public class InputPlayer
{
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public string Height { get; set; }
    public string Weight { get; set; }
    public string? HeadshotURL { get; set; }

    public CSHLPlayer ToCSHLPlayer() => new()
    {
        Id = Guid.NewGuid(),
        Name = Name,
        Birthday = Birthday,
        Height = Height,
        Weight = Weight,
        HeadshotUrl = HeadshotURL ?? CSHLTheme.EmptyProfileUrl,
    };
}
