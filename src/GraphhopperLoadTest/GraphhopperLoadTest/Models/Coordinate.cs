using System.Globalization;

namespace GraphhopperLoadTest.Models;

public record Coordinate(double Latitude, double Longitude)
{
    public override string ToString() => $"{Latitude.ToString("F6", CultureInfo.InvariantCulture)},{Longitude.ToString("F6", CultureInfo.InvariantCulture)}";
}
