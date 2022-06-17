using FluentValidation.Results;
using storage.Data;

namespace storage.Validators;

public static class ValidationErrorsFormatter
{
    public static object AsBadRequest(this ValidationResult validationResult)
    {
        var result = new Dictionary<string, string>();

        foreach (var error in validationResult.Errors)
        {
            result[error.PropertyName] = error.ErrorMessage;
        }

        return ApiExecutionResult.FromErrors(result);
    }
}