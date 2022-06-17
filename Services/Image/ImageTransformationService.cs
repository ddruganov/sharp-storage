namespace storage.Services.Image;

using System.Security.Cryptography;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Repositories;
using Data;
using SharpImage = SixLabors.ImageSharp.Image;

public class ImageTransformationService
{
    private readonly ImageRepository _imageRepository;

    public ImageTransformationService(ImageRepository imageRepository)
    {
        _imageRepository = imageRepository;
    }

    public async Task<string> Transform(Data.Image image, ImageTransformationData transformationData)
    {
        if (transformationData.Empty)
        {
            return image.RelativePath;
        }

        var modifiedImageHash = GetModifiedImageHash(image, transformationData);

        var existingModifiedImage = await _imageRepository.GetModified(modifiedImageHash);
        if (existingModifiedImage is not null)
        {
            return existingModifiedImage.RelativePath;
        }

        var (relativeModifiedFilepath, absoluteModifiedFilepath) = GetFilePaths(modifiedImageHash);

        await SaveToDisk(image, transformationData, absoluteModifiedFilepath);

        return await SaveToDatabase(image, modifiedImageHash, relativeModifiedFilepath);
    }

    private async Task<string> SaveToDatabase(Data.Image image, string modifiedImageHash, string relativeModifiedFilepath)
    {
        var modifiedImage = new ModifiedImage()
        {
            Uuid = image.Uuid,
            Hash = modifiedImageHash,
            RelativePath = relativeModifiedFilepath
        };

        await _imageRepository.CreateModified(modifiedImage);

        return modifiedImage.RelativePath;
    }

    private static async Task SaveToDisk(Data.Image image, ImageTransformationData transformationData,
        string absoluteModifiedFilepath)
    {
        await using var outputFileStream = File.Create(absoluteModifiedFilepath);

        using var sharpImageStream =
            await SharpImage.LoadAsync(Path.Combine(Environment.CurrentDirectory, image.RelativePath));
        var imageClone = sharpImageStream.Clone(
            i => i.Resize(new ResizeOptions()
            {
                Size = new Size(transformationData.Width, transformationData.Height),
                Mode = ResizeMode.Crop,
                Sampler = new NearestNeighborResampler()
            })
        );

        await imageClone.SaveAsync(outputFileStream, new WebpEncoder());
    }

    private static (string relativeModifiedFilepath, string absoluteModifiedFilepath) GetFilePaths(
        string modifiedImageHash)
    {
        var modifiedImageFilename = $"{modifiedImageHash}.webp";

        var relativeModifiedFolder = Path.Combine(
            "upload",
            "modified",
            "image",
            DateTime.Now.ToString("yyyy/MM/dd")
        );

        var absoluteModifiedFolder = Path.Combine(Environment.CurrentDirectory, relativeModifiedFolder);

        Directory.CreateDirectory(absoluteModifiedFolder);

        var relativeModifiedFilepath = Path.Combine(relativeModifiedFolder, modifiedImageFilename);
        var absoluteModifiedFilepath = Path.Combine(absoluteModifiedFolder, modifiedImageFilename);

        return (relativeModifiedFilepath, absoluteModifiedFilepath);
    }

    private static string GetModifiedImageHash(Data.Image image, ImageTransformationData transformationData)
    {
        var transformationInfo = $"{image.Hash}:{transformationData.Width}x{transformationData.Height}";
        return
            Convert.ToHexString(SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(transformationInfo))).ToLower();
    }
}