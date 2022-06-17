namespace storage.Data;

public class ModifiedImage
{
    public Guid Uuid { get; init; }
    public string Hash { get; init; } = null!;
    public string RelativePath { get; init; } = null!;
}