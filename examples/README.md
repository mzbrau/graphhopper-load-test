# Example Usage

This directory contains example files and usage scenarios for the GraphHopper Load Test tool.

## Quick Test (No GraphHopper Server Required)

To see the tool in action without setting up GraphHopper, you can run a quick test that will fail quickly but still generate a report:

```bash
dotnet run -- --duration 1 --request-delay 100 --url "http://localhost:9999"
```

This will:
- Run for 1 minute
- Make requests every 100ms  
- Attempt to connect to a non-existent server (localhost:9999)
- Generate a report showing the failed requests

## Real GraphHopper Test

If you have GraphHopper running locally:

```bash
# Basic test with default settings
dotnet run

# Quick 2-minute test
dotnet run -- --duration 2 --center-point "40.7128,-74.0060"

# High-frequency test
dotnet run -- --duration 3 --request-delay 200 --thread-interval 1
```

## Expected Output Structure

After running, you'll get:
- Console logs showing progress
- An HTML report file (`load-test-results.html` by default)
- Statistics including response times, success rates, and detailed logs
