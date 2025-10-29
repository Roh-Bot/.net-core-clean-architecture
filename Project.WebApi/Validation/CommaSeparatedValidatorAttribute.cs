using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Project.WebApi.Validation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class CommaSeparatedValidatorAttribute : ValidationAttribute
{
    private static readonly Regex _regex = new(@"^([a-zA-Z0-9_\-]+)(,[a-zA-Z0-9_\-]+)*$", RegexOptions.Compiled);

    public CommaSeparatedValidatorAttribute()
    {
        ErrorMessage = "Tags must be a single value or comma-separated values without other separators.";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return ValidationResult.Success;

        var str = value.ToString();

        if (string.IsNullOrWhiteSpace(str) || _regex.IsMatch(str))
            return ValidationResult.Success;

        return new ValidationResult(ErrorMessage);
    }
}