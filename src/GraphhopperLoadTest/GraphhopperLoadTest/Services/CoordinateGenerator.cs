using GraphhopperLoadTest.Models;

namespace GraphhopperLoadTest.Services;

public interface ICoordinateGenerator
{
    Coordinate GenerateTargetCoordinate(Coordinate centerPoint, double radiusKm);
    Coordinate GenerateSourceCoordinate(Coordinate targetPoint, double minRadiusKm, double maxRadiusKm);
}

public class CoordinateGenerator : ICoordinateGenerator
{
    private readonly Random _random = new();

    public Coordinate GenerateTargetCoordinate(Coordinate centerPoint, double radiusKm)
    {
        return GenerateRandomCoordinateInRadius(centerPoint, radiusKm);
    }

    public Coordinate GenerateSourceCoordinate(Coordinate targetPoint, double minRadiusKm, double maxRadiusKm)
    {
        var radius = _random.NextDouble() * (maxRadiusKm - minRadiusKm) + minRadiusKm;
        return GenerateRandomCoordinateInRadius(targetPoint, radius);
    }

    private Coordinate GenerateRandomCoordinateInRadius(Coordinate center, double radiusKm)
    {
        // Convert radius from kilometers to degrees (rough approximation)
        var radiusDegrees = radiusKm / 111.0; // 1 degree ≈ 111 km

        // Generate random angle and distance
        var angle = _random.NextDouble() * 2 * Math.PI;
        var distance = Math.Sqrt(_random.NextDouble()) * radiusDegrees;

        // Calculate new coordinates
        var lat = center.Latitude + (distance * Math.Cos(angle));
        var lng = center.Longitude + (distance * Math.Sin(angle) / Math.Cos(center.Latitude * Math.PI / 180));

        // Ensure coordinates are within valid bounds
        lat = Math.Max(-90.0, Math.Min(90.0, lat));
        lng = Math.Max(-180.0, Math.Min(180.0, lng));

        return new Coordinate(lat, lng);
    }
}
