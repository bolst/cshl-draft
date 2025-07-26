namespace CSHLDraft.Data;


public class CSHLPlayer : IEquatable<CSHLPlayer>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime Birthday { get; set; }
    public string Height { get; set; }
    public string Weight { get; set; }
    public string HeadshotUrl { get; set; }
    public string Position { get; set; }
    public Guid draft_id { get; set; }
    
    #region IEquatable
    
    public bool Equals(CSHLPlayer? other) => other is not null && other.Id == Id;
    public override bool Equals(object? obj) => Equals(obj as CSHLPlayer);
    public override int GetHashCode() => Id.GetHashCode();
    public static bool operator == (CSHLPlayer? left, CSHLPlayer? right) => left is null ? right is null : left.Equals(right);
    
    public static bool operator != (CSHLPlayer? left, CSHLPlayer? right) => left is null ? right is not null : !left.Equals(right);

    #endregion
}