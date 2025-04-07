using System.Net;

namespace PaymentGateway.Api.Models.Responses;

public class ApiResponse<T>
{
    public bool IsSuccess { get; init; }
    public HttpStatusCode StatusCode { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public string? RawResponse { get; init; }

    public static ApiResponse<T> Success(T data, HttpStatusCode statusCode, string? raw = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = true,
            StatusCode = statusCode,
            Data = data,
            RawResponse = raw
        };
    }

    public static ApiResponse<T> Failure(HttpStatusCode statusCode, string? errorMessage = null, string? raw = null)
    {
        return new ApiResponse<T>
        {
            IsSuccess = false,
            StatusCode = statusCode,
            ErrorMessage = errorMessage,
            RawResponse = raw
        };
    }
}