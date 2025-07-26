namespace CSHLDraft.Data;

public class InputTeam
{
    public string? LogoUrl { get; set; }
    public string Name { get; set; }
    public int Pick { get; set; }


    public CSHLTeam ToCSHLTeam() => new()
    {
        Id = Guid.NewGuid(),
        Name = Name,
        Pick = Pick,
        LogoUrl = LogoUrl ?? CSHLTheme.LogoUrl,
    };
}
