using Microsoft.AspNetCore.Mvc;
using PaymentGateway.Api.ApiClients;
using PaymentGateway.Api.Models;
using PaymentGateway.Api.Models.Responses;
using PaymentGateway.Api.Services;

using Refit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<PaymentsRepository>();
builder.Services.AddScoped<BankProcessor>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errorResponse = new PostPaymentResponse
        {
            Id = Guid.NewGuid(),
            Status = PaymentStatus.Rejected,
            Error = "Validation failed: " + string.Join("; ", context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
        };

        return new BadRequestObjectResult(errorResponse);
    };
});

var bankApiBaseUrl = builder.Configuration["BankApi:BaseUrl"] ?? "http://localhost:8080";

builder.Services.AddRefitClient<IBankApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(bankApiBaseUrl));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
