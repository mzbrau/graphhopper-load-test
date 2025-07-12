namespace GraphhopperLoadTest.Models;

public class LoadTestStatistics
{
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public TimeSpan MinResponseTime { get; set; }
    public TimeSpan MaxResponseTime { get; set; }
    public double StandardDeviation { get; set; }
    public DateTime TestStartTime { get; set; }
    public DateTime TestEndTime { get; set; }
    public List<RouteResponse> AllResponses { get; set; } = [];
    public Dictionary<int, RouteResponse> FirstSuccessfulResponsePerThread { get; set; } = [];
    
    public double SuccessRate => TotalRequests > 0 ? (double)SuccessfulRequests / TotalRequests * 100 : 0;
}
