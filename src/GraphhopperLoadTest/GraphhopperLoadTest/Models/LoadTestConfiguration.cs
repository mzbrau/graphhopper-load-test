namespace GraphhopperLoadTest.Models;

public class LoadTestConfiguration
{
    public string GraphhopperUrl { get; set; } = "http://localhost:8989";
    public Coordinate CenterPoint { get; set; } = new(51.5074, -0.1278); // London
    public int TestDurationMinutes { get; set; } = 10;
    public int ThreadStartIntervalMinutes { get; set; } = 1;
    public int RequestDelayMilliseconds { get; set; } = 1000;
    public double TargetRadiusKm { get; set; } = 5.0;
    public double SourceRadiusMinKm { get; set; } = 40.0;
    public double SourceRadiusMaxKm { get; set; } = 50.0;
    public string OutputFile { get; set; } = "load-test-results.html"; // Note: timestamp will be added automatically
    public bool IncludeInstructions { get; set; } = true;
    public string? TestName { get; set; }
}
