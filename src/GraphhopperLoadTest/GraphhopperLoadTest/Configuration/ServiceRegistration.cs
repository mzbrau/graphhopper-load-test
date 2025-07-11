using GraphhopperLoadTest.Commands;
using GraphhopperLoadTest.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GraphhopperLoadTest.Configuration;

public static class ServiceRegistration
{
    public static IServiceCollection AddLoadTestServices(this IServiceCollection services)
    {
        // Register HTTP client
        services.AddHttpClient<IGraphhopperClient, GraphhopperClient>();
        services.AddHttpClient<LoadTestCommand>();

        // Register application services
        services.AddSingleton<ICoordinateGenerator, CoordinateGenerator>();
        services.AddSingleton<ILoadTestRunner, LoadTestRunner>();
        services.AddSingleton<IReportGenerator, ReportGenerator>();

        // Register command handlers
        services.AddTransient<LoadTestCommand>();

        // Configure logging (will be set to Information level by default)
        services.AddLogging(config =>
        {
            config.AddConsole();
            config.SetMinimumLevel(LogLevel.Information);
        });

        return services;
    }
}
