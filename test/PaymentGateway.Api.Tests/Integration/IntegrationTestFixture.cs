using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace PaymentGateway.Api.Tests.Integration;

public class IntegrationTestFixture : IAsyncLifetime
{
    private const int ApiPort = 5067;
    private const int BankSimulatorPort = 8080;
    
    public HttpClient Client { get; private set; } = default!;
    public IContainer BankSimulator { get; private set; } = default!;
    public IContainer PaymentGatewayApi { get; private set; } = default!;
    public INetwork Network { get; private set; } = default!;
    
    public async Task InitializeAsync()
    {
        Network = new NetworkBuilder()
            .WithName(Guid.NewGuid().ToString("N")) // random name avoids collisions
            .Build();
        
        BankSimulator = new ContainerBuilder()
            .WithImage("bbyars/mountebank:2.8.1")
            .WithNetwork(Network)
            .WithNetworkAliases("bank_simulator")
            .WithName("test-bank-simulator")
            .WithPortBinding(2525, 2525)
            .WithPortBinding(BankSimulatorPort, BankSimulatorPort)
            .WithBindMount(Path.GetFullPath("../../../../../imposters"), "/imposters")
            .WithEntrypoint("mb")
            //.WithCommand("--configfile /imposters/bank_simulator.ejs --allowInjection")
            .WithEntrypoint("/bin/sh", "-c")
            .WithCommand("while [ ! -f /imposters/bank_simulator.ejs ]; do echo waiting; sleep 0.5; done; mb --configfile /imposters/bank_simulator.ejs --allowInjection")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(2525))
            .Build();

        await BankSimulator.StartAsync();

        var image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(CommonDirectoryPath.GetSolutionDirectory(), string.Empty)
            .WithDockerfile("Dockerfile")
            // .WithDeleteIfExists(true)
            // .WithName("test-payment-gateway-image") 
            .Build();
        
        await image.CreateAsync()
            .ConfigureAwait(false);
        
        // Start the Payment Gateway API
        PaymentGatewayApi = new ContainerBuilder()
            .WithImage(image)
            .WithNetwork(Network)
            .WithName("test-payment-gateway")
            .WithPortBinding(ApiPort, ApiPort)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
            .WithEnvironment("ASPNETCORE_URLS", $"http://+:{ApiPort}")
            .WithEnvironment("BankApi__BaseUrl", $"http://bank_simulator:{BankSimulatorPort}")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(ApiPort))
            .DependsOn(BankSimulator)
            .Build();
        
        await PaymentGatewayApi.StartAsync();
        
        Client = new HttpClient
        {
            BaseAddress = new Uri($"http://localhost:{ApiPort}")
        };
    }

    public async Task DisposeAsync()
    {
        await PaymentGatewayApi.StopAsync();
        await BankSimulator.StopAsync();
        await Network.DeleteAsync();
    }
}