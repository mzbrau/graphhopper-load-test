namespace GraphhopperLoadTest.Models;

public class RouteResponse
{
    public RouteRequest Request { get; set; } = default!;
    public TimeSpan ResponseTime { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CompletedAt { get; set; }
}
