using PaymentGateway.Api.Models.Requests;
using Refit;
using BankPaymentResponse = PaymentGateway.Api.Models.Responses.BankPaymentResponse;

namespace PaymentGateway.Api.ApiClients;

public interface IBankApi
{
    [Post("/payments")]
    Task<ApiResponse<BankPaymentResponse>?> ProcessPayment([Body] BankPaymentRequest request);
}