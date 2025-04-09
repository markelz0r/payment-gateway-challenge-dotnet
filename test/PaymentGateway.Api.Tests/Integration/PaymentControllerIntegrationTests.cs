using System.Net;
using System.Net.Http.Json;

using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Tests.Integration;

[Collection("IntegrationTests")]
public class PaymentGatewayIntegrationTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task PostPayment_ShouldReturnAuthorized()
    {
        const string cardNumber = "4012888888881881";
        const int expiryMonth = 5;
        const int expiryYear = 2030;
        const string currency = "USD";
        const int amount = 100;
        const string cvv = "123";
        
        var request = new
        {
            cardNumber,
            expiryMonth,
            expiryYear,
            currency,
            amount,
            cvv
        };

        var response = await fixture.Client.PostAsJsonAsync("/api/payments", request);

        var content = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        Assert.Equal(cardNumber[^4..], content!.CardNumberLastFour);
        Assert.Equal(expiryMonth, content.ExpiryMonth);
        Assert.Equal(expiryYear, content.ExpiryYear);
        Assert.Equal(currency, content.Currency);
        Assert.Equal(amount, content.Amount);
        
        Assert.Equal(PaymentStatus.Authorized, content!.Status);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
    
    [Fact]
    public async Task PostPayment_ShouldReturnDeclined()
    {
        var request = new
        {
            cardNumber = "4012888888881882",
            expiryMonth = 12,
            expiryYear = 2030,
            currency = "USD",
            amount = 100,
            cvv = "123"
        };

        var response = await fixture.Client.PostAsJsonAsync("/api/payments", request);
        var content = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        Assert.Equal(PaymentStatus.Declined, content!.Status);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPayment_ShouldReturnRecentAuthorisedPayment()
    {
        const string cardNumber = "4012888888881881";
        const int expiryMonth = 5;
        const int expiryYear = 2030;
        const string currency = "USD";
        const int amount = 100;
        const string cvv = "123";
        
        var request = new
        {
            cardNumber,
            expiryMonth,
            expiryYear,
            currency,
            amount,
            cvv
        };
        
        var response = await fixture.Client.PostAsJsonAsync("/api/payments", request);
        var content = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var id = content!.Id;

        var repositoryResponse = await fixture.Client.GetAsync($"api/Payments/{id}");
        var repositoryContent = await repositoryResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        Assert.Equal(HttpStatusCode.OK, repositoryResponse.StatusCode);
        Assert.Equal(id, repositoryContent!.Id);
        Assert.Equal(cardNumber[^4..], repositoryContent.CardNumberLastFour);
        Assert.Equal(expiryMonth, repositoryContent.ExpiryMonth);
        Assert.Equal(expiryYear, repositoryContent.ExpiryYear);
        Assert.Equal(currency, repositoryContent.Currency);
        Assert.Equal(amount, repositoryContent.Amount);
        Assert.Equal(PaymentStatus.Authorized, repositoryContent.Status);
    }
    
    [Fact]
    public async Task GetPayment_ShouldReturnRecentDeclinedPayment()
    {
        const string cardNumber = "4012888888881882";
        const int expiryMonth = 5;
        const int expiryYear = 2030;
        const string currency = "USD";
        const int amount = 100;
        const string cvv = "123";
        
        var request = new
        {
            cardNumber,
            expiryMonth,
            expiryYear,
            currency,
            amount,
            cvv
        };
        
        var response = await fixture.Client.PostAsJsonAsync("/api/payments", request);
        var content = await response.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var id = content!.Id;

        var repositoryResponse = await fixture.Client.GetAsync($"api/Payments/{id}");
        var repositoryContent = await repositoryResponse.Content.ReadFromJsonAsync<PostPaymentResponse>();
        
        Assert.Equal(HttpStatusCode.OK, repositoryResponse.StatusCode);
        Assert.Equal(id, repositoryContent!.Id);
        Assert.Equal(cardNumber[^4..], repositoryContent.CardNumberLastFour);
        Assert.Equal(expiryMonth, repositoryContent.ExpiryMonth);
        Assert.Equal(expiryYear, repositoryContent.ExpiryYear);
        Assert.Equal(currency, repositoryContent.Currency);
        Assert.Equal(amount, repositoryContent.Amount);
        Assert.Equal(PaymentStatus.Declined, repositoryContent.Status);
    }
    
    [Fact]
    public async Task GetPayment_ShouldReturnNotFound()
    {
        var repositoryResponse = await fixture.Client.GetAsync($"api/Payments/123");
        
        Assert.Equal(HttpStatusCode.NotFound, repositoryResponse.StatusCode);
    }
}