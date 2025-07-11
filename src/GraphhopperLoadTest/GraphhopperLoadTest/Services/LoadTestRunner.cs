using System.Collections.Concurrent;
using GraphhopperLoadTest.Models;
using Microsoft.Extensions.Logging;

namespace GraphhopperLoadTest.Services;

public interface ILoadTestRunner
{
    Task<LoadTestStatistics> RunLoadTestAsync(LoadTestConfiguration configuration, CancellationToken cancellationToken = default);
}

public class LoadTestRunner : ILoadTestRunner
{
    private readonly IGraphhopperClient _graphhopperClient;
    private readonly ICoordinateGenerator _coordinateGenerator;
    private readonly ILogger<LoadTestRunner> _logger;
    private readonly ConcurrentBag<RouteResponse> _responses = [];

    public LoadTestRunner(
        IGraphhopperClient graphhopperClient,
        ICoordinateGenerator coordinateGenerator,
        ILogger<LoadTestRunner> logger)
    {
        _graphhopperClient = graphhopperClient;
        _coordinateGenerator = coordinateGenerator;
        _logger = logger;
    }

    public async Task<LoadTestStatistics> RunLoadTestAsync(LoadTestConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation("Starting load test at {StartTime}", startTime);
        _logger.LogInformation("Test will run for {Duration} minutes", configuration.TestDurationMinutes);
        _logger.LogInformation("GraphHopper URL: {Url}", configuration.GraphhopperUrl);
        _logger.LogInformation("Center point: {CenterPoint}", configuration.CenterPoint);

        var testEndTime = startTime.AddMinutes(configuration.TestDurationMinutes);
        var tasks = new List<Task>();
        var threadCount = 0;

        // Start threads at intervals
        while (DateTime.UtcNow < testEndTime && !cancellationToken.IsCancellationRequested)
        {
            threadCount++;
            var threadId = threadCount;
            var targetCoordinate = _coordinateGenerator.GenerateTargetCoordinate(
                configuration.CenterPoint, 
                configuration.TargetRadiusKm);

            _logger.LogInformation("Starting thread {ThreadId} with target {Target}", threadId, targetCoordinate);

            var task = RunThreadAsync(threadId, targetCoordinate, testEndTime, configuration, cancellationToken);
            tasks.Add(task);

            // Wait for the next thread start time
            var nextThreadStartTime = startTime.AddMinutes(threadCount * configuration.ThreadStartIntervalMinutes);
            var delayUntilNextThread = nextThreadStartTime - DateTime.UtcNow;
            
            if (delayUntilNextThread > TimeSpan.Zero && DateTime.UtcNow < testEndTime)
            {
                await Task.Delay(delayUntilNextThread, cancellationToken);
            }
        }

        _logger.LogInformation("All {ThreadCount} threads started. Waiting for completion...", threadCount);
        await Task.WhenAll(tasks);

        var endTime = DateTime.UtcNow;
        _logger.LogInformation("Load test completed at {EndTime}", endTime);

        return CalculateStatistics(startTime, endTime);
    }

    private async Task RunThreadAsync(int threadId, Coordinate targetCoordinate, DateTime endTime, LoadTestConfiguration configuration, CancellationToken cancellationToken)
    {
        var requestCount = 0;
        var consecutiveFailures = 0;
        const int maxConsecutiveFailures = 5;
        
        while (DateTime.UtcNow < endTime && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                requestCount++;
                
                // Generate source coordinate with retry logic for invalid coordinates
                var sourceCoordinate = await GenerateValidSourceCoordinateAsync(
                    targetCoordinate, 
                    configuration, 
                    cancellationToken);

                if (sourceCoordinate == null)
                {
                    _logger.LogWarning("Thread {ThreadId}: Unable to generate valid source coordinate after retries. Skipping request {RequestCount}", 
                        threadId, requestCount);
                    consecutiveFailures++;
                    if (consecutiveFailures >= maxConsecutiveFailures)
                    {
                        _logger.LogWarning("Thread {ThreadId}: Too many consecutive failures. Stopping thread.", threadId);
                        break;
                    }
                    await Task.Delay(configuration.RequestDelayMilliseconds, cancellationToken);
                    continue;
                }

                var request = new RouteRequest
                {
                    Source = sourceCoordinate,
                    Target = targetCoordinate,
                    RequestTime = DateTime.UtcNow,
                    ThreadId = threadId
                };

                var response = await _graphhopperClient.GetRouteAsync(request, configuration, cancellationToken);
                _responses.Add(response);

                if (response.IsSuccess)
                {
                    consecutiveFailures = 0; // Reset on successful request
                }
                else
                {
                    consecutiveFailures++;
                    _logger.LogDebug("Thread {ThreadId}: Request failed: {Error}", threadId, response.ErrorMessage);
                }

                if (requestCount % 10 == 0)
                {
                    _logger.LogDebug("Thread {ThreadId}: Completed {RequestCount} requests. Last response time: {ResponseTime}ms", 
                        threadId, requestCount, response.ResponseTime.TotalMilliseconds);
                }

                // Stop thread if too many consecutive failures
                if (consecutiveFailures >= maxConsecutiveFailures)
                {
                    _logger.LogWarning("Thread {ThreadId}: Too many consecutive failures. Stopping thread.", threadId);
                    break;
                }

                // Wait before next request
                if (DateTime.UtcNow < endTime)
                {
                    await Task.Delay(configuration.RequestDelayMilliseconds, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in thread {ThreadId} during request {RequestCount}", threadId, requestCount);
                consecutiveFailures++;
                if (consecutiveFailures >= maxConsecutiveFailures)
                {
                    _logger.LogWarning("Thread {ThreadId}: Too many consecutive failures. Stopping thread.", threadId);
                    break;
                }
            }
        }

        _logger.LogInformation("Thread {ThreadId} completed with {RequestCount} requests", threadId, requestCount);
    }

    private async Task<Coordinate?> GenerateValidSourceCoordinateAsync(
        Coordinate targetCoordinate, 
        LoadTestConfiguration configuration, 
        CancellationToken cancellationToken,
        int maxAttempts = 3)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            var sourceCoordinate = _coordinateGenerator.GenerateSourceCoordinate(
                targetCoordinate, 
                configuration.SourceRadiusMinKm, 
                configuration.SourceRadiusMaxKm);

            // Quick validation: test if we can route from source to target
            var testRequest = new RouteRequest
            {
                Source = sourceCoordinate,
                Target = targetCoordinate,
                RequestTime = DateTime.UtcNow,
                ThreadId = -1 // Special ID for validation requests
            };

            try
            {
                var testResponse = await _graphhopperClient.GetRouteAsync(testRequest, configuration, cancellationToken);
                if (testResponse.IsSuccess)
                {
                    return sourceCoordinate;
                }
                
                // If this is not the last attempt, try again
                if (attempt < maxAttempts)
                {
                    _logger.LogDebug("Coordinate validation failed on attempt {Attempt}, retrying...", attempt);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Error during coordinate validation on attempt {Attempt}", attempt);
            }
        }

        return null; // All attempts failed
    }

    private LoadTestStatistics CalculateStatistics(DateTime startTime, DateTime endTime)
    {
        var allResponses = _responses.ToList();
        var successfulResponses = allResponses.Where(r => r.IsSuccess).ToList();
        
        var statistics = new LoadTestStatistics
        {
            TestStartTime = startTime,
            TestEndTime = endTime,
            TotalRequests = allResponses.Count,
            SuccessfulRequests = successfulResponses.Count,
            FailedRequests = allResponses.Count - successfulResponses.Count,
            AllResponses = allResponses
        };

        if (successfulResponses.Any())
        {
            var responseTimes = successfulResponses.Select(r => r.ResponseTime.TotalMilliseconds).ToList();
            
            statistics.AverageResponseTime = TimeSpan.FromMilliseconds(responseTimes.Average());
            statistics.MinResponseTime = TimeSpan.FromMilliseconds(responseTimes.Min());
            statistics.MaxResponseTime = TimeSpan.FromMilliseconds(responseTimes.Max());
            
            var mean = responseTimes.Average();
            var variance = responseTimes.Select(x => Math.Pow(x - mean, 2)).Average();
            statistics.StandardDeviation = Math.Sqrt(variance);
        }

        _logger.LogInformation("Test Statistics:");
        _logger.LogInformation("Total Requests: {Total}", statistics.TotalRequests);
        _logger.LogInformation("Successful Requests: {Successful}", statistics.SuccessfulRequests);
        _logger.LogInformation("Failed Requests: {Failed}", statistics.FailedRequests);
        _logger.LogInformation("Success Rate: {SuccessRate:F2}%", statistics.SuccessRate);
        _logger.LogInformation("Average Response Time: {Average:F2}ms", statistics.AverageResponseTime.TotalMilliseconds);
        _logger.LogInformation("Min Response Time: {Min:F2}ms", statistics.MinResponseTime.TotalMilliseconds);
        _logger.LogInformation("Max Response Time: {Max:F2}ms", statistics.MaxResponseTime.TotalMilliseconds);
        _logger.LogInformation("Standard Deviation: {StdDev:F2}ms", statistics.StandardDeviation);

        return statistics;
    }
}
