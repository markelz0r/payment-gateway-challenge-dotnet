using System.ComponentModel.DataAnnotations;
using PaymentGateway.Api.Models.Requests;

namespace PaymentGateway.Api.Tests.Models;

public class PostPaymentRequestTests
{
    [Fact]
    public void ValidModel_ShouldPassValidation()
    {
        var model = new PostPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            Currency = "USD",
            Amount = 100,
            Cvv = "023"
        };

        var results = ValidateModel(model);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData("abcd1234")]     // Non-numeric
    [InlineData("123")]          // Too short
    [InlineData("12345678901234567890")] // Too long
    public void InvalidCardNumber_ShouldFail(string cardNumber)
    {
        var model = new PostPaymentRequest
        {
            CardNumber = cardNumber,
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        var results = ValidateModel(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(PostPaymentRequest.CardNumber)));
    }

    [Fact]
    public void ExpiryDateInPast_ShouldFail()
    {
        var model = new PostPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 1,
            ExpiryYear = 2000,
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };

        var results = ValidateModel(model);

        Assert.Contains(results, r => r.ErrorMessage!.Contains("expiry date must be in the future"));
    }

    [Theory]
    [InlineData("12")]      // Too short
    [InlineData("12345")]   // Too long
    [InlineData("abc")]     // Not numeric
    public void InvalidCvv_ShouldFail(string cvv)
    {
        var model = new PostPaymentRequest
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 12,
            ExpiryYear = DateTime.UtcNow.Year + 1,
            Currency = "USD",
            Amount = 100,
            Cvv = cvv
        };

        var results = ValidateModel(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(PostPaymentRequest.Cvv)));
    }
    
    private static IEnumerable<ValidationResult> ValidateModel(PostPaymentRequest model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }
}