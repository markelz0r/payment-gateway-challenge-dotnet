using System.Net;
using Moq;
using PaymentGateway.Api.ApiClients;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

using Refit;

namespace PaymentGateway.Api.Tests.Services
{
    public class BankProcessorTests
    {
        private readonly Mock<IBankApi> _bankApiMock;
        private readonly BankProcessor _processor;

        public BankProcessorTests()
        {
            _bankApiMock = new Mock<IBankApi>();
            _processor = new BankProcessor(_bankApiMock.Object);
        }
        
        [Fact]
        public async Task ProcessPayment_ShouldReturnAuthorized_WhenBankResponseIsAuthorized()
        {
            // Arrange
            var request = CreateValidRequest();
            var response = new BankPaymentResponse(true, Guid.NewGuid().ToString());

            _bankApiMock.Setup(api => api.ProcessPayment(It.IsAny<BankPaymentRequest>()))
                .ReturnsAsync(new ApiResponse<BankPaymentResponse>(
                    new HttpResponseMessage(HttpStatusCode.OK),
                    response,
                    new RefitSettings()
                ));

            // Act
            var result = await _processor.ProcessPayment(request);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(PaymentStatus.Authorized, result.Status);
        }

        [Fact]
        public async Task ProcessPayment_ShouldReturnDeclined_WhenBankResponseIsNotAuthorized()
        {
            var request = CreateValidRequest();
            var response = new BankPaymentResponse(false, Guid.NewGuid().ToString());

            _bankApiMock.Setup(api => api.ProcessPayment(It.IsAny<BankPaymentRequest>()))
                .ReturnsAsync(new ApiResponse<BankPaymentResponse>(
                    new HttpResponseMessage(HttpStatusCode.OK),
                    response,
                    new RefitSettings()
                ));

            var result = await _processor.ProcessPayment(request);

            Assert.True(result.IsSuccess);
            Assert.Equal(PaymentStatus.Declined, result.Status);
        }

        [Fact]
        public async Task ProcessPayment_ShouldFail_WhenApiReturnsNull()
        {
            var request = CreateValidRequest();

            _bankApiMock.Setup(api => api.ProcessPayment(It.IsAny<BankPaymentRequest>()))
                .ReturnsAsync((ApiResponse<BankPaymentResponse>)null!);

            var result = await _processor.ProcessPayment(request);

            Assert.False(result.IsSuccess);
            Assert.Equal(PaymentStatus.Declined, result.Status);
            Assert.Equal("Bank API didn't return a result", result.ErrorMessage);
        }

        [Fact]
        public async Task ProcessPayment_ShouldFail_WhenApiResponseIsUnsuccessful()
        {
            var request = CreateValidRequest();

            var httpResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

            _bankApiMock.Setup(api => api.ProcessPayment(It.IsAny<BankPaymentRequest>()))
                .ReturnsAsync(new ApiResponse<BankPaymentResponse>(
                    httpResponse,
                    null!,
                    new RefitSettings()
                ));

            var result = await _processor.ProcessPayment(request);

            Assert.False(result.IsSuccess);
            Assert.Equal(PaymentStatus.Declined, result.Status);
            Assert.Contains("Bank API error", result.ErrorMessage);
        }

        [Fact]
        public async Task ProcessPayment_ShouldFail_WhenApiReturnsNullContent()
        {
            var request = CreateValidRequest();

            _bankApiMock.Setup(api => api.ProcessPayment(It.IsAny<BankPaymentRequest>()))
                .ReturnsAsync(new ApiResponse<BankPaymentResponse>(
                    new HttpResponseMessage(HttpStatusCode.OK),
                    null!,
                    new RefitSettings()
                ));

            var result = await _processor.ProcessPayment(request);

            Assert.False(result.IsSuccess);
            Assert.Equal(PaymentStatus.Declined, result.Status);
            Assert.Equal("Bank API returned empty response.", result.ErrorMessage);
        }

        [Fact]
        public async Task ProcessPayment_ShouldFail_WhenApiThrowsException()
        {
            var request = CreateValidRequest();

            _bankApiMock.Setup(api => api.ProcessPayment(It.IsAny<BankPaymentRequest>()))
                .ThrowsAsync(new Exception("timeout"));

            var result = await _processor.ProcessPayment(request);

            Assert.False(result.IsSuccess);
            Assert.Equal(PaymentStatus.Declined, result.Status);
            Assert.Contains("Bank API client has thrown an exception:", result.ErrorMessage);
        }
        
        private static PostPaymentRequest CreateValidRequest() => new()
        {
            CardNumber = "4111111111111111",
            ExpiryMonth = 12,
            ExpiryYear = 2030,
            Currency = "USD",
            Amount = 100,
            Cvv = "123"
        };
    }
}
