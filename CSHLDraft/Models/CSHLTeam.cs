namespace CSHLDraft.Data;


public class CSHLTeam : IEquatable<CSHLTeam>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string LogoUrl { get; set; }
    public Guid draft_id { get; set; }
    public int Pick { get; set; }
    public string PrimaryHex { get; set; } = "#BC1B20";
    public string SecondaryHex { get; set; } = "#FFFFFF";
    public int? GmAccountId { get; set; }
    public string? GmName { get; set; }

    #region IEquatable
    
    public bool Equals(CSHLTeam? other) => other is not null && other.Id == Id;
    public override bool Equals(object? obj) => Equals(obj as CSHLTeam);
    public override int GetHashCode() => Id.GetHashCode();
    public static bool operator == (CSHLTeam? left, CSHLTeam? right) => left is null ? right is null : left.Equals(right);
    
    public static bool operator != (CSHLTeam? left, CSHLTeam? right) => left is null ? right is not null : !left.Equals(right);

    #endregion
}