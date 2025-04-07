using PaymentGateway.Api.Models;

namespace PaymentGateway.Api.Services;

public record PaymentProcessingResult
{
    public bool IsSuccess { get; init; }
    public string? ErrorMessage { get; init; }

    public PaymentStatus? Status { get; init; }

    public static PaymentProcessingResult Success(PaymentStatus status) =>
        new() { IsSuccess = true, Status = status};

    public static PaymentProcessingResult Failure(string error, PaymentStatus status) =>
        new() { IsSuccess = false, ErrorMessage = error, Status = status};
}