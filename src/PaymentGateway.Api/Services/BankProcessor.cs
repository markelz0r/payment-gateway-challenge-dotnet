using PaymentGateway.Api.ApiClients;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;

namespace PaymentGateway.Api.Services;

public class BankProcessor(IBankApi bankApi)
{
    public async Task<PaymentProcessingResult> ProcessPayment(PostPaymentRequest request)
    {
        var bankPaymentRequest = CreateBankPaymentRequest(request);
        Refit.ApiResponse<BankPaymentResponse>? result;
        try
        {
            result = await bankApi.ProcessPayment(bankPaymentRequest);
        }
        catch (Exception e)
        {
            return PaymentProcessingResult.Failure($"Bank API client has thrown an exception: {e.Message}", PaymentStatus.Rejected);
        }
        
        // PaymentStatus.Rejected could be used here to handle and track bad API responses
        if (result == null)
        {
            return PaymentProcessingResult.Failure($"Bank API didn't return a result", PaymentStatus.Rejected);
        }
        
        if (!result.IsSuccessStatusCode)
            return PaymentProcessingResult.Failure($"Bank API error: {result.StatusCode}", PaymentStatus.Rejected);
        
        if (result.Content == null)
        {
            return PaymentProcessingResult.Failure("Bank API returned empty response.", PaymentStatus.Rejected);
        }

        var status = result.Content.Authorized ? PaymentStatus.Authorized : PaymentStatus.Declined;
        
        return PaymentProcessingResult.Success(status);
    }

    private static BankPaymentRequest CreateBankPaymentRequest(PostPaymentRequest request)
    {
        var expiryMonthString = request.ExpiryMonth.ToString("D2");
        var expiryYearString = request.ExpiryYear!.ToString();

        return new BankPaymentRequest(
            request.CardNumber,
            $"{expiryMonthString}/{expiryYearString}",
            request.Currency,
            request.Amount,
            request.Cvv.ToString());
    }
}