using System.Text.Json;
using GraphhopperLoadTest.Models;
using Microsoft.Extensions.Logging;

namespace GraphhopperLoadTest.Services;

public interface IGraphhopperClient
{
    Task<RouteResponse> GetRouteAsync(RouteRequest request, LoadTestConfiguration configuration, CancellationToken cancellationToken = default);
}

public class GraphhopperClient : IGraphhopperClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GraphhopperClient> _logger;

    public GraphhopperClient(HttpClient httpClient, ILogger<GraphhopperClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<RouteResponse> GetRouteAsync(RouteRequest request, LoadTestConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var response = new RouteResponse
        {
            Request = request,
            CompletedAt = DateTime.UtcNow
        };

        try
        {
            var baseUrl = configuration.GraphhopperUrl.TrimEnd('/');
            var sourceCoordString = request.Source.ToString();
            var targetCoordString = request.Target.ToString();
            
            var url = $"{baseUrl}/route?" +
                     $"point={sourceCoordString}" +
                     $"&point={targetCoordString}" +
                     "&profile=car" +
                     $"&instructions={configuration.IncludeInstructions.ToString().ToLower()}" +
                     "&calc_points=true" +
                     "&points_encoded=false";

            _logger.LogDebug("Making request to: {Url}", url);
            _logger.LogDebug("Source coordinate: {Source}, Target coordinate: {Target}", sourceCoordString, targetCoordString);

            var httpResponse = await _httpClient.GetAsync(url, cancellationToken);
            stopwatch.Stop();

            response.ResponseTime = stopwatch.Elapsed;
            response.IsSuccess = httpResponse.IsSuccessStatusCode;

            if (!httpResponse.IsSuccessStatusCode)
            {
                var errorContent = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                response.ErrorMessage = $"HTTP {httpResponse.StatusCode}: {errorContent}";
                _logger.LogWarning("Request failed: {Error}", response.ErrorMessage);
            }
            else
            {
                // Validate that we got a valid JSON response
                var content = await httpResponse.Content.ReadAsStringAsync(cancellationToken);
                try
                {
                    using var jsonDoc = JsonDocument.Parse(content);
                    // Just verify it's valid JSON, we don't need to process the route details
                }
                catch (JsonException ex)
                {
                    response.IsSuccess = false;
                    response.ErrorMessage = $"Invalid JSON response: {ex.Message}";
                    _logger.LogWarning("Invalid JSON response: {Error}", ex.Message);
                }
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            response.ResponseTime = stopwatch.Elapsed;
            response.IsSuccess = false;
            response.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Error making route request");
        }

        return response;
    }
}
