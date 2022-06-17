using System.Security.Cryptography;
using SixLabors.ImageSharp.Formats.Webp;
using storage.Repositories;
using SharpImage = SixLabors.ImageSharp.Image;

namespace storage.Services.Image;

public sealed class ImageImageUploadService : IImageUploadService
{
    private readonly ImageRepository _imageRepository;

    public ImageImageUploadService(ImageRepository imageRepository)
    {
        _imageRepository = imageRepository;
    }

    public async Task<Guid?> UploadImageAsync(IFormFile image)
    {
        var imageHash = await GetImageHash(image);

        var existingImage = await _imageRepository.Get(imageHash);
        if (existingImage is not null)
        {
            return existingImage.Uuid;
        }

        var (relativeFilepath, absoluteFilepath) = GetFilePaths(imageHash);

        await SaveImageToDisk(image, absoluteFilepath);

        return await SaveImageToDatabase(image, imageHash, relativeFilepath);
    }

    private async Task<Guid?> SaveImageToDatabase(IFormFile image, string imageHash, string relativeFilepath)
    {
        var uuid = Guid.NewGuid();
        var insertResult = await _imageRepository.Create(new Data.Image()
        {
            Uuid = uuid,
            Size = image.Length,
            Hash = imageHash,
            RelativePath = relativeFilepath
        });

        return insertResult ? uuid : null;
    }

    private static (string relativeFilepath, string absoluteFilepath) GetFilePaths(string imageHash)
    {
        var filename = $"{imageHash}.webp";

        var relativeFolder = Path.Combine(
            "upload",
            "image",
            DateTime.Now.ToString("yyyy/MM/dd")
        );

        var absoluteFolder = Path.Combine(
            Environment.CurrentDirectory,
            relativeFolder
        );

        Directory.CreateDirectory(absoluteFolder);

        var relativeFilepath = Path.Combine(relativeFolder, filename);
        var absoluteFilepath = Path.Combine(absoluteFolder, filename);
        return (relativeFilepath, absoluteFilepath);
    }

    private static async Task SaveImageToDisk(IFormFile image, string absoluteFilepath)
    {
        var sharpImageStream = await SharpImage.LoadAsync(image.OpenReadStream());
        await using var outputFileStream = File.Create(absoluteFilepath);
        await sharpImageStream.SaveAsync(outputFileStream, new WebpEncoder());
    }

    private static async Task<string> GetImageHash(IFormFile image)
    {
        var imageStream = image.OpenReadStream();
        var imageHash = Convert.ToHexString(await SHA1.Create().ComputeHashAsync(imageStream)).ToLower();
        imageStream.Close();
        return imageHash;
    }
}