using FluentValidation;
using FluentValidation.Results;

namespace storage.Validators;

public class UploadedImageValidator : AbstractValidator<IFormFile?>
{
    private static readonly string[] Conditions = { "image/jpeg", "image/png" };
    private const int _50MB = 52428800;

    public UploadedImageValidator()
    {
        RuleFor(image => image!.ContentType)
            .Must(contentType => Conditions.Contains(contentType))
            .WithMessage("Invalid content type");

        RuleFor(image => image!.Length)
            .LessThan(_50MB)
            .WithMessage("Image too large");
    }

    protected override bool PreValidate(ValidationContext<IFormFile?> context, ValidationResult result)
    {
        if (context.InstanceToValidate is not null)
        {
            return base.PreValidate(context, result);
        }

        result.Errors.Add(new ValidationFailure("common", "Image not provided"));
        return false;
    }
}