using System.Net;
using System.Net.Http.Json;

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace PaymentGateway.Api.Tests.Integration;

public class PaymentGatewayIntegrationTests : IAsyncLifetime
{
    private const int ApiPort = 5067;
    private const int BankSimulatorPort = 8080;

    private IContainer _bankSimulator;
    private IContainer _paymentGatewayApi;

    public async Task InitializeAsync()
    {
        _bankSimulator = new ContainerBuilder()
            .WithImage("bbyars/mountebank:2.8.1")
            .WithWorkingDirectory(CommonDirectoryPath.GetSolutionDirectory().DirectoryPath)
            .WithName("test-bank-simulator")
            .WithPortBinding(2525, 2525)
            .WithPortBinding(BankSimulatorPort, BankSimulatorPort)
            .WithBindMount(Path.GetFullPath("imposters"), "/imposters")
            .WithCommand("--configfile /imposters/bank_simulator.ejs --allowInjection")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(2525))
            .Build();

        await _bankSimulator.StartAsync();

        var image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
            .WithDockerfile("Dockerfile")
            // .WithDeleteIfExists(true)
            // .WithName("test-payment-gateway-image") 
            .Build();
        
        await image.CreateAsync()
            .ConfigureAwait(false);
        
        // Start the Payment Gateway API
        _paymentGatewayApi = new ContainerBuilder()
            .WithImage(image)
            .WithName("test-payment-gateway")
            .WithPortBinding(ApiPort, ApiPort)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("ASPNETCORE_URLS", $"http://+:{ApiPort}")
            .WithEnvironment("BankApi__BaseUrl", $"http://test-bank-simulator:{BankSimulatorPort}")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(ApiPort))
            .DependsOn(_bankSimulator)
            .Build();
        
        await _paymentGatewayApi.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _paymentGatewayApi.StopAsync();
        await _bankSimulator.StopAsync();
    }
    
    [Fact]
    public async Task PostPayment_ShouldReturnAuthorized()
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri($"http://localhost:{ApiPort}");

        var request = new
        {
            cardNumber = "4012888888881881",
            expiryMonth = 12,
            expiryYear = 2030,
            currency = "USD",
            amount = 100,
            cvv = "123"
        };

        var response = await httpClient.PostAsJsonAsync("/api/payments", request);
        
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Authorized", content);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}