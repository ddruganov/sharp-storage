namespace storage.Services.Image;

public interface IImageUploadService
{
    public Task<Guid?> UploadImageAsync(IFormFile image);
}