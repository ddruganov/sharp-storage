namespace storage.Data;

public class Image
{
    public Guid Uuid { get; init; }
    public long Size { get; init; }
    public string Hash { get; init; } = null!;
    public string RelativePath { get; init; } = null!;
}