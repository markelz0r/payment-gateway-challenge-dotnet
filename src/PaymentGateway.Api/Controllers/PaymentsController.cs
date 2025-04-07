﻿using Microsoft.AspNetCore.Mvc;
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
        if (!processingResult.IsSuccess)
        {
            return BadRequest(new
            {
                error = processingResult.ErrorMessage
            });
        }

        var postPaymentResponse = CreatePostPaymentResponse(request, processingResult.Status!.Value);
        paymentsRepository.Add(postPaymentResponse);
        
        return Ok(postPaymentResponse);
    }
    
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PostPaymentResponse?>> GetPaymentAsync(Guid id)
    {
        var payment = paymentsRepository.Get(id);

        return new OkObjectResult(payment);
    }
    
    private static PostPaymentResponse CreatePostPaymentResponse(PostPaymentRequest request, PaymentStatus status)
    {
        return new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            Currency = request.Currency,
            CardNumberLastFour = int.Parse(request.CardNumber[^4..]),
            ExpiryMonth = request.ExpiryMonth,
            ExpiryYear = request.ExpiryYear,
            Status = status
        };
    }
}