using System.ComponentModel.DataAnnotations;
using PaymentGateway.Api.Models.Validation;

namespace PaymentGateway.Api.Tests.Models
{
    public class IsoCurrencyAttributeTests
    {
        private readonly IsoCurrencyAttribute _attribute = new();

        [Theory]
        [InlineData("USD")]
        [InlineData("eur")]
        [InlineData("GbP")]
        public void ValidCurrencyCodes_ShouldPass(string currency)
        {
            // Act
            var result = _attribute.GetValidationResult(currency, new ValidationContext(new object()));

            // Assert
            Assert.Equal(ValidationResult.Success, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("ABC")]
        [InlineData("usdollars")]
        [InlineData("EURO")]
        [InlineData("RUB")]
        public void InvalidCurrencyCodes_ShouldFail(string currency)
        {
            // Act
            var result = _attribute.GetValidationResult(currency, new ValidationContext(new object()));

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Invalid ISO currency code.", result!.ErrorMessage);
        }
    }
}