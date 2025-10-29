using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Project.WebApi.Validation;
public class DateFormatAttribute : ValidationAttribute
{
    private readonly string _format;

    public DateFormatAttribute(string format)
    {
        _format = format;
        ErrorMessage = $"Date must be in format {_format}";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        switch (value)
        {
            case null:
            case string str when DateTime.TryParseExact(str, _format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _):
                return ValidationResult.Success;
            case string str:
                return new ValidationResult(ErrorMessage);
            default:
                return new ValidationResult("Invalid data type for date");
        }
    }
}