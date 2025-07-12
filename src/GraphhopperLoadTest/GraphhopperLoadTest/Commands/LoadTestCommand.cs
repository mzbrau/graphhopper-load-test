using Cocona;
using GraphhopperLoadTest.Models;
using GraphhopperLoadTest.Services;
using Microsoft.Extensions.Logging;

namespace GraphhopperLoadTest.Commands;

public class LoadTestCommand
{
    private readonly IGraphhopperClient _graphhopperClient;
    private readonly ILoadTestRunner _loadTestRunner;
    private readonly IReportGenerator _reportGenerator;
    private readonly ILogger<LoadTestCommand> _logger;
    private readonly HttpClient _httpClient;
    private readonly ILoggerFactory _loggerFactory;

    public LoadTestCommand(
        IGraphhopperClient graphhopperClient,
        ILoadTestRunner loadTestRunner,
        IReportGenerator reportGenerator,
        ILogger<LoadTestCommand> logger,
        HttpClient httpClient,
        ILoggerFactory loggerFactory)
    {
        _graphhopperClient = graphhopperClient;
        _loadTestRunner = loadTestRunner;
        _reportGenerator = reportGenerator;
        _logger = logger;
        _httpClient = httpClient;
        _loggerFactory = loggerFactory;
    }

    [Command]
    public async Task<int> RunLoadTestAsync(
        [Option('u', Description = "GraphHopper server URL")] string url = "http://localhost:8989",
        [Option('c', Description = "Center point latitude,longitude. Examples: London(51.5074,-0.1278), Berlin(52.5200,13.4050), NYC(40.7128,-74.0060)")] string centerPoint = "51.5074,-0.1278",
        [Option('d', Description = "Test duration in minutes")] int duration = 10,
        [Option('i', Description = "Thread start interval in minutes")] int threadInterval = 1,
        [Option('r', Description = "Request delay in milliseconds")] int requestDelay = 1000,
        [Option('t', Description = "Target radius in kilometers")] double targetRadius = 5.0,
        [Option('s', Description = "Source radius minimum in kilometers")] double sourceRadiusMin = 40.0,
        [Option('S', Description = "Source radius maximum in kilometers")] double sourceRadiusMax = 50.0,
        [Option('o', Description = "Output HTML file path")] string output = "load-test-results.html",
        [Option("no-instructions", Description = "Disable route instructions in requests")] bool noInstructions = false,
        [Option('v', Description = "Enable verbose logging")] bool verbose = false,
        [Option('n', Description = "Test name to display in the report")] string? testName = null,
        [Option("validate-coordinates", Description = "Validate that center point has accessible routing data")] bool validateCoordinates = true)
    {
        try
        {
            // Parse center point
            var configuration = ParseAndValidateConfiguration(
                url, centerPoint, duration, threadInterval, requestDelay,
                targetRadius, sourceRadiusMin, sourceRadiusMax, output, noInstructions, testName);

            if (configuration == null)
                return 1;

            if (verbose)
            {
                _logger.LogInformation("Verbose logging enabled");
                // Note: In a future enhancement, we could dynamically adjust log levels here
                // For now, verbose logging would need to be configured at startup
            }

            DisplayConfiguration(configuration);

            // Test GraphHopper connectivity
            if (!await TestConnectivityAsync(configuration.GraphhopperUrl))
            {
                _logger.LogWarning("GraphHopper connectivity test failed, but continuing anyway...");
            }

            // Validate center point coordinates if requested
            if (validateCoordinates)
            {
                if (!await ValidateCenterPointAsync(configuration))
                {
                    _logger.LogError("Center point validation failed. The specified coordinates may not have accessible routing data.");
                    _logger.LogInformation("Try using coordinates in a well-connected urban area, or disable validation with --validate-coordinates=false");
                    return 1;
                }
            }

            // Run the load test
            _logger.LogInformation("Starting load test...");
            _logger.LogInformation("Press Ctrl+C to stop the test early.");

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
                _logger.LogInformation("Stopping load test...");
            };

            var statistics = await _loadTestRunner.RunLoadTestAsync(configuration, cts.Token);

            // Generate report
            _logger.LogInformation("Generating HTML report...");
            await _reportGenerator.GenerateHtmlReportAsync(statistics, configuration);

            var fullPath = Path.GetFullPath(configuration.OutputFile);
            _logger.LogInformation("Load test completed successfully!");
            _logger.LogInformation("Results saved to: {OutputPath}", fullPath);

            // Open the report in the default browser
            try
            {
                _logger.LogInformation("Opening report in default browser...");
                OpenInDefaultBrowser(fullPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to open report in browser: {Message}", ex.Message);
                _logger.LogInformation("You can manually open the report at: {OutputPath}", fullPath);
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during load test: {Message}", ex.Message);
            return 1;
        }
    }

    private LoadTestConfiguration? ParseAndValidateConfiguration(
        string url, string centerPoint, int duration, int threadInterval, int requestDelay,
        double targetRadius, double sourceRadiusMin, double sourceRadiusMax, string output, bool noInstructions, string? testName)
    {
        // Parse center point
        var centerCoords = centerPoint.Split(',');
        if (centerCoords.Length != 2 ||
            !double.TryParse(centerCoords[0], out var lat) ||
            !double.TryParse(centerCoords[1], out var lng))
        {
            _logger.LogError("Invalid center point format. Use latitude,longitude (e.g., 51.5074,-0.1278)");
            return null;
        }

        // Validate parameters
        if (duration <= 0)
        {
            _logger.LogError("Duration must be greater than 0");
            return null;
        }

        if (threadInterval <= 0)
        {
            _logger.LogError("Thread interval must be greater than 0");
            return null;
        }

        if (sourceRadiusMin >= sourceRadiusMax)
        {
            _logger.LogError("Source radius minimum must be less than maximum");
            return null;
        }

        // Add timestamp to filename if using default name
        var outputFile = output;
        if (output == "load-test-results.html")
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            outputFile = $"load-test-results_{timestamp}.html";
        }

        return new LoadTestConfiguration
        {
            GraphhopperUrl = url,
            CenterPoint = new Coordinate(lat, lng),
            TestDurationMinutes = duration,
            ThreadStartIntervalMinutes = threadInterval,
            RequestDelayMilliseconds = requestDelay,
            TargetRadiusKm = targetRadius,
            SourceRadiusMinKm = sourceRadiusMin,
            SourceRadiusMaxKm = sourceRadiusMax,
            OutputFile = outputFile,
            IncludeInstructions = !noInstructions,
            TestName = testName
        };
    }

    private void DisplayConfiguration(LoadTestConfiguration configuration)
    {
        Console.WriteLine("GraphHopper Load Test Tool");
        Console.WriteLine("==========================");
        if (!string.IsNullOrWhiteSpace(configuration.TestName))
        {
            Console.WriteLine($"Test Name: {configuration.TestName}");
        }
        Console.WriteLine($"GraphHopper URL: {configuration.GraphhopperUrl}");
        Console.WriteLine($"Center Point: {configuration.CenterPoint}");
        Console.WriteLine($"Test Duration: {configuration.TestDurationMinutes} minutes");
        Console.WriteLine($"Thread Start Interval: {configuration.ThreadStartIntervalMinutes} minute(s)");
        Console.WriteLine($"Request Delay: {configuration.RequestDelayMilliseconds}ms");
        Console.WriteLine($"Target Radius: {configuration.TargetRadiusKm}km");
        Console.WriteLine($"Source Radius: {configuration.SourceRadiusMinKm}-{configuration.SourceRadiusMaxKm}km");
        Console.WriteLine($"Include Instructions: {configuration.IncludeInstructions}");
        Console.WriteLine($"Output File: {configuration.OutputFile}");
        Console.WriteLine();
        Console.WriteLine("Note: For best results, use center coordinates in well-connected urban areas");
        Console.WriteLine("with good road networks. Avoid remote areas, water bodies, or regions");
        Console.WriteLine("without GraphHopper map data coverage.");
        Console.WriteLine();
    }

    private async Task<bool> TestConnectivityAsync(string graphhopperUrl)
    {
        try
        {
            _logger.LogInformation("Testing GraphHopper connectivity...");
            var testResponse = await _httpClient.GetAsync($"{graphhopperUrl}/health", CancellationToken.None);
            
            if (testResponse.IsSuccessStatusCode)
            {
                _logger.LogInformation("GraphHopper connectivity test passed.");
                return true;
            }
            
            _logger.LogWarning("GraphHopper health check failed (HTTP {StatusCode})", testResponse.StatusCode);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to test GraphHopper connectivity: {Message}", ex.Message);
            return false;
        }
    }

    private async Task<bool> ValidateCenterPointAsync(LoadTestConfiguration configuration)
    {
        try
        {
            _logger.LogInformation("Validating center point coordinates...");
            
            // Generate a small test route near the center point
            var testTarget = new Coordinate(
                configuration.CenterPoint.Latitude + 0.01, // ~1km offset
                configuration.CenterPoint.Longitude + 0.01);

            var testRequest = new RouteRequest
            {
                Source = configuration.CenterPoint,
                Target = testTarget
            };

            var testResponse = await _graphhopperClient.GetRouteAsync(testRequest, configuration, CancellationToken.None);
            
            if (testResponse.IsSuccess)
            {
                _logger.LogInformation("Center point validation passed.");
                return true;
            }
            
            _logger.LogWarning("Center point validation failed: {Error}", testResponse.ErrorMessage);
            
            // Try a second test with a different offset to be sure
            var testTarget2 = new Coordinate(
                configuration.CenterPoint.Latitude - 0.01,
                configuration.CenterPoint.Longitude - 0.01);

            var testRequest2 = new RouteRequest
            {
                Source = configuration.CenterPoint,
                Target = testTarget2
            };

            var testResponse2 = await _graphhopperClient.GetRouteAsync(testRequest2, configuration, CancellationToken.None);
            
            if (testResponse2.IsSuccess)
            {
                _logger.LogInformation("Center point validation passed on second attempt.");
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate center point: {Message}", ex.Message);
            return false;
        }
    }

    private static void OpenInDefaultBrowser(string filePath)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            else if (OperatingSystem.IsMacOS())
            {
                System.Diagnostics.Process.Start("open", filePath);
            }
            else if (OperatingSystem.IsLinux())
            {
                System.Diagnostics.Process.Start("xdg-open", filePath);
            }
            else
            {
                throw new PlatformNotSupportedException("Opening files in browser is not supported on this platform");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to open file in default browser: {ex.Message}", ex);
        }
    }
}
