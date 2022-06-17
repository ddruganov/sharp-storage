namespace storage.Data;

public class ImageTransformationData
{
    public int Width { get; init; }
    public int Height { get; init; }

    public bool Empty => Width == default && Height == default;
}