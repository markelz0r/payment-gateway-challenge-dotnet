using System.ComponentModel.DataAnnotations;
using PaymentGateway.Api.Models.Validation;

namespace PaymentGateway.Api.Models.Requests;

public record PostPaymentRequest : IValidatableObject
{
    [Required]
    //[CreditCard] // This is an overkill as per requirements but is quite neat way to ensure ISO/IEC 7812 compliance
    [RegularExpression("^[0-9]+$", ErrorMessage = "Card number must be numeric.")]
    [StringLength(19, MinimumLength = 14, ErrorMessage = "Card number must be between 14 and 19 digits.")]
    public required string CardNumber { get; init; }

    [Required, Range(1, 12)]
    public required int ExpiryMonth { get; init; }

    [Required, Range(1, 9998)]
    public required int ExpiryYear { get; init; }

    [Required, IsoCurrency]
    public required string Currency { get; init; }

    [Required, Range(1, int.MaxValue)]
    public required int Amount { get; init; }

    [Required]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits.")]
    public required string Cvv { get; init; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var expiryDate = new DateTime(ExpiryYear, ExpiryMonth, 1).AddMonths(1).AddDays(-1);

        if (expiryDate < DateTime.UtcNow)
        {
            yield return new ValidationResult(
                "The expiry date must be in the future.",
                new[] { nameof(ExpiryMonth), nameof(ExpiryYear) }
            );
        }
    }
}