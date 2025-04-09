using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Requests;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

namespace PaymentGateway.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentsController(PaymentsRepository paymentsRepository, BankProcessor bankProcessor)
    : Controller
{
    [HttpPost]
    public async Task<ActionResult<PostPaymentResponse>> PostPaymentAsync([FromBody] PostPaymentRequest request)
    {
        var processingResult = await bankProcessor.ProcessPayment(request);
        var postPaymentResponse = CreatePostPaymentResponse(request, processingResult.Status, processingResult.ErrorMessage);
        
        if (!processingResult.IsSuccess)
        {
            return new BadRequestObjectResult(postPaymentResponse);
        }
        
        paymentsRepository.Add(postPaymentResponse);
        return Ok(postPaymentResponse);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = paymentsRepository.Get(id);
        if (payment == null)
            return new NotFoundResult();
        
        return new OkObjectResult(payment);
    }
    
    private static PostPaymentResponse CreatePostPaymentResponse(
        PostPaymentRequest request, 
        PaymentStatus status,
        string? errorMessage)
    {
        return new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            Currency = request.Currency,
            CardNumberLastFour = request.CardNumber[^4..],
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Status = status,
            Error = errorMessage
        };
    }
}