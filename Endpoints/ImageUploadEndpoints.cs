using Microsoft.AspNetCore.Mvc;
using storage.Data;
using storage.Repositories;
using storage.Services.Image;
using storage.Validators;

namespace storage.Endpoints;

public static class ImageUploadEndpoints
{
    public static void MapImageUploadEndpoints(this WebApplication app)
    {
        app.MapPost("/upload/image", UploadImage);

        app.MapGet("/image/{uuid:guid}", GetImage);
    }

    private static async Task<object> UploadImage([FromServices] IImageUploadService imageUploadService,
        [FromServices] UploadedImageValidator validator,
        HttpRequest request)
    {
        IFormFile? image = null;
        try
        {
            image = request.Form.Files.FirstOrDefault();
        }
        catch
        {
            // ignored
        }

        var validationResult = await validator.ValidateAsync(image);
        if (!validationResult.IsValid)
        {
            return validationResult.AsBadRequest();
        }

        var guid = await imageUploadService.UploadImageAsync(image!);

        return guid is null
            ? ApiExecutionResult.FromException("Couldn't save image to disk")
            : ApiExecutionResult.SuccessData(new { guid });
    }

    private static async Task<IResult> GetImage([FromServices] ImageRepository imageRepository,
        [FromServices] ImageTransformationService imageTransformationService, Guid uuid,
        HttpContext httpContext, [FromQuery] int? width, [FromQuery] int? height)
    {
        var image = await imageRepository.Get(uuid);
        if (image is null)
        {
            return Results.NotFound();
        }

        var transformedImageRelativePath = await imageTransformationService.Transform(image, new ImageTransformationData
        {
            Width = width ?? 0,
            Height = height ?? 0
        });

        return Results.File(
            path: Path.Combine(Environment.CurrentDirectory, transformedImageRelativePath),
            contentType: "image/webp"
        );
    }
}