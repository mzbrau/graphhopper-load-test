using System.Globalization;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Random delay generator
static async Task AddRandomDelay()
{
    var delay = Random.Shared.Next(50, 500); // 50-500ms random delay
    await Task.Delay(delay);
}

// Health check endpoint
app.MapGet("/health", async () =>
{
    await AddRandomDelay();
    return Results.Ok(new { status = "ok" });
});

// Main routing endpoint that simulates GraphHopper
app.MapGet("/route", async (
    string? point,
    HttpContext context) =>
{
    await AddRandomDelay();
    
    var queryParams = context.Request.Query;
    var points = queryParams["point"].ToArray();
    
    if (points.Length < 2)
    {
        return Results.BadRequest(new 
        { 
            message = "At least 2 points required",
            hints = new[] 
            {
                new { message = "At least 2 points required", details = "QueryParam" }
            }
        });
    }

    // Parse coordinates from points
    var coordinates = new List<double[]>();
    foreach (var p in points)
    {
        var pointString = p?.ToString();
        if (string.IsNullOrEmpty(pointString)) continue;
        
        var coords = pointString.Split(',');
        if (coords.Length == 2 && 
            double.TryParse(coords[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var lat) && 
            double.TryParse(coords[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var lng))
        {
            coordinates.Add([lng, lat]); // GraphHopper uses lng,lat order in response
        }
    }

    if (coordinates.Count < 2)
    {
        return Results.BadRequest(new 
        { 
            message = "Invalid coordinates format",
            hints = new[] 
            {
                new { message = "Invalid coordinates format", details = "QueryParam" }
            }
        });
    }

    // Get optional parameters
    var vehicle = queryParams["vehicle"].FirstOrDefault() ?? "car";
    var instructions = queryParams["instructions"].FirstOrDefault() ?? "true";
    var calcPoints = queryParams["calc_points"].FirstOrDefault() ?? "true";

    // Calculate realistic distance and time based on coordinates
    var distance = CalculateDistance(coordinates[0], coordinates[1]);
    var timeMs = CalculateTime(distance, vehicle);

    // Create realistic response
    var response = new
    {
        paths = new[]
        {
            new
            {
                distance = Math.Round(distance, 1),
                weight = timeMs / 1000.0,
                time = timeMs,
                transfers = 0,
                points_encoded = calcPoints.ToLower() == "true",
                bbox = new[] 
                {
                    Math.Min(coordinates[0][0], coordinates[1][0]) - 0.01,
                    Math.Min(coordinates[0][1], coordinates[1][1]) - 0.01,
                    Math.Max(coordinates[0][0], coordinates[1][0]) + 0.01,
                    Math.Max(coordinates[0][1], coordinates[1][1]) + 0.01
                },
                points = calcPoints.ToLower() == "true" ? GenerateRoutePoints(coordinates) : new {},
                instructions = instructions.ToLower() == "true" ? GenerateInstructions(distance) : [],
                legs = new object[0],
                details = new {},
                ascend = Random.Shared.NextDouble() * 100,
                descend = Random.Shared.NextDouble() * 100,
                snapped_waypoints = new
                {
                    type = "LineString",
                    coordinates = coordinates
                }
            }
        },
        info = new
        {
            copyrights = new[] { "GraphHopper Simulator" },
            took = Random.Shared.Next(5, 50)
        }
    };

    return Results.Ok(response);
});

// Info endpoint
app.MapGet("/info", async () =>
{
    await AddRandomDelay();
    return Results.Ok(new
    {
        version = "8.0",
        build_date = "2024-01-01T00:00:00Z",
        features = new
        {
            routing = true,
            matrix = true,
            isochrone = true,
            map_matching = false
        },
        supported_vehicles = new[] { "car", "bike", "foot", "motorcycle" },
        data_date = "2024-01-01T00:00:00Z",
        import_date = "2024-01-01T00:00:00Z"
    });
});

app.Run();

// Helper functions
static double CalculateDistance(double[] point1, double[] point2)
{
    // Haversine formula for distance calculation
    const double R = 6371000; // Earth's radius in meters
    
    var lat1Rad = point1[1] * Math.PI / 180;
    var lat2Rad = point2[1] * Math.PI / 180;
    var deltaLatRad = (point2[1] - point1[1]) * Math.PI / 180;
    var deltaLngRad = (point2[0] - point1[0]) * Math.PI / 180;

    var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
            Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
            Math.Sin(deltaLngRad / 2) * Math.Sin(deltaLngRad / 2);
    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

    return R * c;
}

static long CalculateTime(double distanceMeters, string vehicle)
{
    // Average speeds in km/h
    var speeds = new Dictionary<string, double>
    {
        { "car", 50 },
        { "bike", 20 },
        { "foot", 5 },
        { "motorcycle", 60 }
    };

    var speed = speeds.GetValueOrDefault(vehicle, 50);
    var timeHours = (distanceMeters / 1000.0) / speed;
    var timeMs = (long)(timeHours * 3600 * 1000);
    
    // Add some randomness (±20%)
    var variation = Random.Shared.NextDouble() * 0.4 - 0.2; // -20% to +20%
    return (long)(timeMs * (1 + variation));
}

static object GenerateRoutePoints(List<double[]> coordinates)
{
    // Generate a simple encoded polyline representation
    var points = new List<double[]>();
    
    // Add start point
    points.Add(coordinates[0]);
    
    // Add some intermediate points for realism
    var numIntermediatePoints = Random.Shared.Next(3, 8);
    for (int i = 0; i < numIntermediatePoints; i++)
    {
        var ratio = (double)(i + 1) / (numIntermediatePoints + 1);
        var lat = coordinates[0][1] + (coordinates[1][1] - coordinates[0][1]) * ratio + 
                  (Random.Shared.NextDouble() - 0.5) * 0.01;
        var lng = coordinates[0][0] + (coordinates[1][0] - coordinates[0][0]) * ratio + 
                  (Random.Shared.NextDouble() - 0.5) * 0.01;
        points.Add([lng, lat]);
    }
    
    // Add end point
    points.Add(coordinates[1]);
    
    return new
    {
        type = "LineString",
        coordinates = points
    };
}

static object[] GenerateInstructions(double distance)
{
    var instructions = new List<object>();
    
    // Start instruction
    instructions.Add(new
    {
        distance = Math.Round(distance * 0.1, 1),
        heading = Random.Shared.NextDouble() * 360,
        sign = 0,
        interval = new[] { 0, 1 },
        text = "Head north",
        time = Random.Shared.Next(1000, 5000)
    });
    
    // Some turn instructions
    var numTurns = Random.Shared.Next(2, 6);
    for (int i = 0; i < numTurns; i++)
    {
        var turnTypes = new[] { "Turn left", "Turn right", "Continue straight", "Keep right", "Keep left" };
        var signs = new[] { -3, -2, -1, 0, 1, 2, 3 };
        
        instructions.Add(new
        {
            distance = Math.Round(distance * Random.Shared.NextDouble() * 0.3, 1),
            heading = Random.Shared.NextDouble() * 360,
            sign = signs[Random.Shared.Next(signs.Length)],
            interval = new[] { i + 1, i + 2 },
            text = turnTypes[Random.Shared.Next(turnTypes.Length)],
            time = Random.Shared.Next(500, 3000)
        });
    }
    
    // Arrival instruction
    instructions.Add(new
    {
        distance = 0.0,
        heading = Random.Shared.NextDouble() * 360,
        sign = 4,
        interval = new[] { instructions.Count, instructions.Count + 1 },
        text = "Arrive at destination",
        time = 0
    });
    
    return instructions.ToArray();
}