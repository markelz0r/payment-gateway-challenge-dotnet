using System.ComponentModel.DataAnnotations;

namespace PaymentGateway.Api.Models.Validation;

public class IsoCurrencyAttribute : ValidationAttribute
{
    private static readonly HashSet<string> IsoCurrencyCodes = ["USD", "EUR", "GBP"];

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var currency = value as string;

        if (!string.IsNullOrWhiteSpace(currency) && IsoCurrencyCodes.Contains(currency.ToUpper()))
        {
            return ValidationResult.Success;
        }
        
        return new ValidationResult("Invalid ISO currency code.");
    }
}