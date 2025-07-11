namespace GraphhopperLoadTest.Models;

public class RouteRequest
{
    public Coordinate Source { get; set; } = default!;
    public Coordinate Target { get; set; } = default!;
    public DateTime RequestTime { get; set; }
    public int ThreadId { get; set; }
}
