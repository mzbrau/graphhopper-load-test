using System.Text;
using GraphhopperLoadTest.Models;
using Microsoft.Extensions.Logging;

namespace GraphhopperLoadTest.Services;

public interface IReportGenerator
{
    Task GenerateHtmlReportAsync(LoadTestStatistics statistics, LoadTestConfiguration configuration);
}

public class ReportGenerator : IReportGenerator
{
    private readonly ILogger<ReportGenerator> _logger;

    public ReportGenerator(ILogger<ReportGenerator> logger)
    {
        _logger = logger;
    }

    public async Task GenerateHtmlReportAsync(LoadTestStatistics statistics, LoadTestConfiguration configuration)
    {
        _logger.LogInformation("Generating HTML report to {OutputPath}", configuration.OutputFile);

        var html = GenerateHtmlContent(statistics, configuration);
        await File.WriteAllTextAsync(configuration.OutputFile, html);

        _logger.LogInformation("HTML report generated successfully");
    }

    private string GenerateHtmlContent(LoadTestStatistics statistics, LoadTestConfiguration configuration)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine("    <title>GraphHopper Load Test Results</title>");
        sb.AppendLine("    <script src=\"https://cdn.jsdelivr.net/npm/chart.js\"></script>");
        sb.AppendLine("    <style>");
        sb.AppendLine("        :root {");
        sb.AppendLine("            --bg-primary: #1a1a1a;");
        sb.AppendLine("            --bg-secondary: #2d2d2d;");
        sb.AppendLine("            --bg-card: #3a3a3a;");
        sb.AppendLine("            --text-primary: #ffffff;");
        sb.AppendLine("            --text-secondary: #b0b0b0;");
        sb.AppendLine("            --accent-primary: #4a9eff;");
        sb.AppendLine("            --accent-success: #28a745;");
        sb.AppendLine("            --accent-error: #dc3545;");
        sb.AppendLine("            --border-color: #555555;");
        sb.AppendLine("        }");
        sb.AppendLine("        body { ");
        sb.AppendLine("            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; ");
        sb.AppendLine("            margin: 0; ");
        sb.AppendLine("            padding: 20px; ");
        sb.AppendLine("            background-color: var(--bg-primary); ");
        sb.AppendLine("            color: var(--text-primary); ");
        sb.AppendLine("            line-height: 1.6; ");
        sb.AppendLine("        }");
        sb.AppendLine("        .container { max-width: 1400px; margin: 0 auto; }");
        sb.AppendLine("        .header { text-align: center; margin-bottom: 40px; }");
        sb.AppendLine("        .header h1 { color: var(--accent-primary); margin-bottom: 10px; font-size: 2.5em; }");
        sb.AppendLine("        .header .test-name { color: var(--accent-primary); font-size: 1.4em; margin-bottom: 15px; font-weight: 600; }");
        sb.AppendLine("        .header .test-info { color: var(--text-secondary); font-size: 1.1em; }");
        sb.AppendLine("        .summary { display: grid; grid-template-columns: repeat(auto-fit, minmax(280px, 1fr)); gap: 20px; margin-bottom: 40px; }");
        sb.AppendLine("        .stat-card { ");
        sb.AppendLine("            background: var(--bg-card); ");
        sb.AppendLine("            padding: 25px; ");
        sb.AppendLine("            border-radius: 12px; ");
        sb.AppendLine("            text-align: center; ");
        sb.AppendLine("            border: 1px solid var(--border-color); ");
        sb.AppendLine("            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.3); ");
        sb.AppendLine("        }");
        sb.AppendLine("        .stat-value { font-size: 2.2em; font-weight: bold; color: var(--accent-primary); margin-bottom: 8px; }");
        sb.AppendLine("        .stat-label { color: var(--text-secondary); font-size: 1.1em; }");
        sb.AppendLine("        .charts-container { display: grid; grid-template-columns: 1fr 1fr; gap: 30px; margin-bottom: 40px; }");
        sb.AppendLine("        .charts-container-three { display: grid; grid-template-columns: 1fr; gap: 30px; margin-bottom: 40px; }");
        sb.AppendLine("        .chart-section { background: var(--bg-card); padding: 25px; border-radius: 12px; border: 1px solid var(--border-color); }");
        sb.AppendLine("        .chart-section h2 { color: var(--accent-primary); margin-top: 0; margin-bottom: 20px; font-size: 1.4em; }");
        sb.AppendLine("        .chart-container { width: 100%; height: 400px; }");
        sb.AppendLine("        @media (max-width: 1024px) {");
        sb.AppendLine("            .charts-container { grid-template-columns: 1fr; }");
        sb.AppendLine("        }");
        sb.AppendLine("        .details-section { background: var(--bg-card); padding: 25px; border-radius: 12px; border: 1px solid var(--border-color); margin-bottom: 30px; }");
        sb.AppendLine("        .details-section h2 { color: var(--accent-primary); margin-top: 0; margin-bottom: 20px; font-size: 1.4em; }");
        sb.AppendLine("        table { width: 100%; border-collapse: collapse; }");
        sb.AppendLine("        th, td { border: 1px solid var(--border-color); padding: 12px; text-align: left; }");
        sb.AppendLine("        th { background-color: var(--bg-secondary); color: var(--text-primary); font-weight: 600; }");
        sb.AppendLine("        td { background-color: var(--bg-card); }");
        sb.AppendLine("        .success { color: var(--accent-success); font-weight: 600; }");
        sb.AppendLine("        .error { color: var(--accent-error); font-weight: 600; }");
        sb.AppendLine("        .details { max-height: 500px; overflow-y: auto; }");
        sb.AppendLine("        .details::-webkit-scrollbar { width: 8px; }");
        sb.AppendLine("        .details::-webkit-scrollbar-track { background: var(--bg-secondary); }");
        sb.AppendLine("        .details::-webkit-scrollbar-thumb { background: var(--border-color); border-radius: 4px; }");
        sb.AppendLine("        .details::-webkit-scrollbar-thumb:hover { background: var(--accent-primary); }");
        sb.AppendLine("    </style>");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("    <div class=\"container\">");
        
        // Header
        sb.AppendLine("        <div class=\"header\">");
        sb.AppendLine("            <h1>GraphHopper Load Test Results</h1>");
        if (!string.IsNullOrWhiteSpace(configuration.TestName))
        {
            sb.AppendLine($"            <div class=\"test-name\">{configuration.TestName}</div>");
        }
        sb.AppendLine($"            <div class=\"test-info\">");
        sb.AppendLine($"                Test Duration: {statistics.TestStartTime:yyyy-MM-dd HH:mm:ss} - {statistics.TestEndTime:yyyy-MM-dd HH:mm:ss} UTC<br>");
        sb.AppendLine($"                Total Test Time: {(statistics.TestEndTime - statistics.TestStartTime).TotalMinutes:F2} minutes");
        sb.AppendLine($"            </div>");
        sb.AppendLine("        </div>");

        // Summary cards
        sb.AppendLine("        <div class=\"summary\">");
        sb.AppendLine("            <div class=\"stat-card\">");
        sb.AppendLine($"                <div class=\"stat-value\">{statistics.TotalRequests}</div>");
        sb.AppendLine("                <div class=\"stat-label\">Total Requests</div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("            <div class=\"stat-card\">");
        sb.AppendLine($"                <div class=\"stat-value\">{statistics.SuccessfulRequests}</div>");
        sb.AppendLine("                <div class=\"stat-label\">Successful Requests</div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("            <div class=\"stat-card\">");
        sb.AppendLine($"                <div class=\"stat-value\">{statistics.SuccessRate:F1}%</div>");
        sb.AppendLine("                <div class=\"stat-label\">Success Rate</div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("            <div class=\"stat-card\">");
        sb.AppendLine($"                <div class=\"stat-value\">{statistics.AverageResponseTime.TotalMilliseconds:F0}ms</div>");
        sb.AppendLine("                <div class=\"stat-label\">Avg Response Time</div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("        </div>");

        // Charts section
        sb.AppendLine("        <div class=\"charts-container\">");
        
        // Requests over time chart
        sb.AppendLine("            <div class=\"chart-section\">");
        sb.AppendLine("                <h2>Requests Over Time</h2>");
        sb.AppendLine("                <div class=\"chart-container\">");
        sb.AppendLine("                    <canvas id=\"requestsOverTimeChart\"></canvas>");
        sb.AppendLine("                </div>");
        sb.AppendLine("            </div>");

        // Response time chart
        sb.AppendLine("            <div class=\"chart-section\">");
        sb.AppendLine("                <h2>Response Time Over Time</h2>");
        sb.AppendLine("                <div class=\"chart-container\">");
        sb.AppendLine("                    <canvas id=\"responseTimeChart\"></canvas>");
        sb.AppendLine("                </div>");
        sb.AppendLine("            </div>");
        
        sb.AppendLine("        </div>");

        // Normal distribution chart section
        sb.AppendLine("        <div class=\"charts-container-three\">");
        sb.AppendLine("            <div class=\"chart-section\">");
        sb.AppendLine("                <h2>Response Time Distribution (Normal Distribution Bell Curve)</h2>");
        sb.AppendLine("                <div class=\"chart-container\">");
        sb.AppendLine("                    <canvas id=\"normalDistributionChart\"></canvas>");
        sb.AppendLine("                </div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("        </div>");

        // Statistics table
        sb.AppendLine("        <div class=\"details-section\">");
        sb.AppendLine("            <h2>Detailed Statistics</h2>");
        sb.AppendLine("            <table>");
        sb.AppendLine("                <tr><th>Metric</th><th>Value</th></tr>");
        sb.AppendLine($"                <tr><td>Total Requests</td><td>{statistics.TotalRequests}</td></tr>");
        sb.AppendLine($"                <tr><td>Successful Requests</td><td class=\"success\">{statistics.SuccessfulRequests}</td></tr>");
        sb.AppendLine($"                <tr><td>Failed Requests</td><td class=\"error\">{statistics.FailedRequests}</td></tr>");
        sb.AppendLine($"                <tr><td>Success Rate</td><td>{statistics.SuccessRate:F2}%</td></tr>");
        sb.AppendLine($"                <tr><td>Average Response Time</td><td>{statistics.AverageResponseTime.TotalMilliseconds:F2} ms</td></tr>");
        sb.AppendLine($"                <tr><td>Min Response Time</td><td>{statistics.MinResponseTime.TotalMilliseconds:F2} ms</td></tr>");
        sb.AppendLine($"                <tr><td>Max Response Time</td><td>{statistics.MaxResponseTime.TotalMilliseconds:F2} ms</td></tr>");
        sb.AppendLine($"                <tr><td>Standard Deviation</td><td>{statistics.StandardDeviation:F2} ms</td></tr>");
        sb.AppendLine("            </table>");
        sb.AppendLine("        </div>");

        // All responses table
        sb.AppendLine("        <div class=\"details-section\">");
        sb.AppendLine("            <h2>All Requests</h2>");
        sb.AppendLine("            <div class=\"details\">");
        sb.AppendLine("                <table>");
        sb.AppendLine("                    <tr><th>Thread</th><th>Request Time</th><th>Response Time (ms)</th><th>Status</th><th>Error</th></tr>");
        
        foreach (var response in statistics.AllResponses.OrderBy(r => r.Request.RequestTime))
        {
            var statusClass = response.IsSuccess ? "success" : "error";
            var status = response.IsSuccess ? "Success" : "Failed";
            var error = response.ErrorMessage ?? "";
            
            sb.AppendLine($"                    <tr>");
            sb.AppendLine($"                        <td>{response.Request.ThreadId}</td>");
            sb.AppendLine($"                        <td>{response.Request.RequestTime:HH:mm:ss.fff}</td>");
            sb.AppendLine($"                        <td>{response.ResponseTime.TotalMilliseconds:F0}</td>");
            sb.AppendLine($"                        <td class=\"{statusClass}\">{status}</td>");
            sb.AppendLine($"                        <td>{error}</td>");
            sb.AppendLine($"                    </tr>");
        }
        
        sb.AppendLine("                </table>");
        sb.AppendLine("            </div>");
        sb.AppendLine("        </div>");

        // First successful responses per thread section
        if (statistics.FirstSuccessfulResponsePerThread.Any())
        {
            sb.AppendLine("        <div class=\"details-section\">");
            sb.AppendLine("            <h2>First Successful Response Per Thread</h2>");
            sb.AppendLine("            <p>The first successful GraphHopper response for each thread (formatted JSON):</p>");
            
            foreach (var kvp in statistics.FirstSuccessfulResponsePerThread.OrderBy(x => x.Key))
            {
                var threadId = kvp.Key;
                var response = kvp.Value;
                
                sb.AppendLine($"            <div class=\"details-section\" style=\"margin-bottom: 20px;\">");
                sb.AppendLine($"                <h3 style=\"color: var(--accent-primary); margin-bottom: 10px;\">Thread {threadId}</h3>");
                sb.AppendLine($"                <p style=\"color: var(--text-secondary); margin-bottom: 10px;\">");
                sb.AppendLine($"                    Request Time: {response.Request.RequestTime:yyyy-MM-dd HH:mm:ss.fff} UTC<br>");
                sb.AppendLine($"                    Response Time: {response.ResponseTime.TotalMilliseconds:F0}ms<br>");
                sb.AppendLine($"                    Source: {response.Request.Source}<br>");
                sb.AppendLine($"                    Target: {response.Request.Target}");
                sb.AppendLine($"                </p>");
                
                if (!string.IsNullOrEmpty(response.JsonResponse))
                {
                    sb.AppendLine("                <pre style=\"");
                    sb.AppendLine("                    background-color: var(--bg-secondary);");
                    sb.AppendLine("                    border: 1px solid var(--border-color);");
                    sb.AppendLine("                    border-radius: 6px;");
                    sb.AppendLine("                    padding: 15px;");
                    sb.AppendLine("                    overflow-x: auto;");
                    sb.AppendLine("                    color: var(--text-primary);");
                    sb.AppendLine("                    font-family: 'Courier New', Consolas, Monaco, monospace;");
                    sb.AppendLine("                    font-size: 12px;");
                    sb.AppendLine("                    line-height: 1.4;");
                    sb.AppendLine("                    max-height: 400px;");
                    sb.AppendLine("                    overflow-y: auto;");
                    sb.AppendLine("                    white-space: pre-wrap;");
                    sb.AppendLine("                    word-break: break-word;");
                    sb.AppendLine("                \"><code>");
                    
                    // Format the JSON response
                    try
                    {
                        using var jsonDoc = System.Text.Json.JsonDocument.Parse(response.JsonResponse);
                        using var stream = new MemoryStream();
                        using var writer = new System.Text.Json.Utf8JsonWriter(stream, new System.Text.Json.JsonWriterOptions 
                        { 
                            Indented = true 
                        });
                        jsonDoc.WriteTo(writer);
                        writer.Flush();
                        var formattedJson = System.Text.Encoding.UTF8.GetString(stream.ToArray());
                        sb.AppendLine(System.Web.HttpUtility.HtmlEncode(formattedJson));
                    }
                    catch
                    {
                        // Fallback to raw JSON if formatting fails
                        sb.AppendLine(System.Web.HttpUtility.HtmlEncode(response.JsonResponse));
                    }
                    
                    sb.AppendLine("</code></pre>");
                }
                else
                {
                    sb.AppendLine("                <p style=\"color: var(--text-secondary);\">No JSON response available</p>");
                }
                
                sb.AppendLine("            </div>");
            }
            
            sb.AppendLine("        </div>");
        }

        // JavaScript for charts
        sb.AppendLine("        <script>");
        sb.AppendLine("            Chart.defaults.color = '#b0b0b0';");
        sb.AppendLine("            Chart.defaults.borderColor = '#555555';");
        sb.AppendLine("            Chart.defaults.backgroundColor = 'rgba(74, 158, 255, 0.1)';");
        sb.AppendLine("");
        
        // Prepare data for charts
        var chartData = statistics.AllResponses
            .OrderBy(r => r.Request.RequestTime)
            .Select((r, i) => new { Response = r, Index = i + 1 })
            .ToList();

        // Requests over time chart
        sb.AppendLine("            const requestsCtx = document.getElementById('requestsOverTimeChart').getContext('2d');");
        sb.AppendLine("            const requestsChart = new Chart(requestsCtx, {");
        sb.AppendLine("                type: 'line',");
        sb.AppendLine("                data: {");
        sb.AppendLine("                    labels: [");
        
        foreach (var item in chartData)
        {
            sb.AppendLine($"                        '{item.Response.Request.RequestTime:HH:mm:ss}',");
        }
        
        sb.AppendLine("                    ],");
        sb.AppendLine("                    datasets: [{");
        sb.AppendLine("                        label: 'Request Number',");
        sb.AppendLine("                        data: [");
        
        foreach (var item in chartData)
        {
            sb.AppendLine($"                            {item.Index},");
        }
        
        sb.AppendLine("                        ],");
        sb.AppendLine("                        borderColor: '#4a9eff',");
        sb.AppendLine("                        backgroundColor: 'rgba(74, 158, 255, 0.1)',");
        sb.AppendLine("                        tension: 0.1,");
        sb.AppendLine("                        fill: true");
        sb.AppendLine("                    }]");
        sb.AppendLine("                },");
        sb.AppendLine("                options: {");
        sb.AppendLine("                    responsive: true,");
        sb.AppendLine("                    maintainAspectRatio: false,");
        sb.AppendLine("                    plugins: {");
        sb.AppendLine("                        legend: { labels: { color: '#b0b0b0' } }");
        sb.AppendLine("                    },");
        sb.AppendLine("                    scales: {");
        sb.AppendLine("                        y: {");
        sb.AppendLine("                            beginAtZero: true,");
        sb.AppendLine("                            title: {");
        sb.AppendLine("                                display: true,");
        sb.AppendLine("                                text: 'Request Number',");
        sb.AppendLine("                                color: '#b0b0b0'");
        sb.AppendLine("                            },");
        sb.AppendLine("                            ticks: { color: '#b0b0b0' },");
        sb.AppendLine("                            grid: { color: '#555555' }");
        sb.AppendLine("                        },");
        sb.AppendLine("                        x: {");
        sb.AppendLine("                            title: {");
        sb.AppendLine("                                display: true,");
        sb.AppendLine("                                text: 'Time',");
        sb.AppendLine("                                color: '#b0b0b0'");
        sb.AppendLine("                            },");
        sb.AppendLine("                            ticks: { color: '#b0b0b0' },");
        sb.AppendLine("                            grid: { color: '#555555' }");
        sb.AppendLine("                        }");
        sb.AppendLine("                    }");
        sb.AppendLine("                }");
        sb.AppendLine("            });");
        sb.AppendLine("");

        // Response time chart
        sb.AppendLine("            const responseCtx = document.getElementById('responseTimeChart').getContext('2d');");
        sb.AppendLine("            const responseChart = new Chart(responseCtx, {");
        sb.AppendLine("                type: 'line',");
        sb.AppendLine("                data: {");
        sb.AppendLine("                    labels: [");
        
        var successfulResponses = chartData.Where(item => item.Response.IsSuccess).ToList();
        foreach (var item in successfulResponses)
        {
            sb.AppendLine($"                        '{item.Response.Request.RequestTime:HH:mm:ss}',");
        }
        
        sb.AppendLine("                    ],");
        sb.AppendLine("                    datasets: [{");
        sb.AppendLine("                        label: 'Response Time (ms)',");
        sb.AppendLine("                        data: [");
        
        foreach (var item in successfulResponses)
        {
            sb.AppendLine($"                            {item.Response.ResponseTime.TotalMilliseconds:F0},");
        }
        
        sb.AppendLine("                        ],");
        sb.AppendLine("                        borderColor: '#28a745',");
        sb.AppendLine("                        backgroundColor: 'rgba(40, 167, 69, 0.1)',");
        sb.AppendLine("                        tension: 0.1");
        sb.AppendLine("                    }]");
        sb.AppendLine("                },");
        sb.AppendLine("                options: {");
        sb.AppendLine("                    responsive: true,");
        sb.AppendLine("                    maintainAspectRatio: false,");
        sb.AppendLine("                    plugins: {");
        sb.AppendLine("                        legend: { labels: { color: '#b0b0b0' } }");
        sb.AppendLine("                    },");
        sb.AppendLine("                    scales: {");
        sb.AppendLine("                        y: {");
        sb.AppendLine("                            beginAtZero: true,");
        sb.AppendLine("                            title: {");
        sb.AppendLine("                                display: true,");
        sb.AppendLine("                                text: 'Response Time (ms)',");
        sb.AppendLine("                                color: '#b0b0b0'");
        sb.AppendLine("                            },");
        sb.AppendLine("                            ticks: { color: '#b0b0b0' },");
        sb.AppendLine("                            grid: { color: '#555555' }");
        sb.AppendLine("                        },");
        sb.AppendLine("                        x: {");
        sb.AppendLine("                            title: {");
        sb.AppendLine("                                display: true,");
        sb.AppendLine("                                text: 'Time',");
        sb.AppendLine("                                color: '#b0b0b0'");
        sb.AppendLine("                            },");
        sb.AppendLine("                            ticks: { color: '#b0b0b0' },");
        sb.AppendLine("                            grid: { color: '#555555' }");
        sb.AppendLine("                        }");
        sb.AppendLine("                    }");
        sb.AppendLine("                }");
        sb.AppendLine("            });");
        sb.AppendLine("");

        // Normal distribution chart
        sb.AppendLine("            // Generate normal distribution chart");
        sb.AppendLine("            const normalCtx = document.getElementById('normalDistributionChart').getContext('2d');");
        sb.AppendLine("            ");
        sb.AppendLine("            // Calculate normal distribution data");
        sb.AppendLine($"            const mean = {statistics.AverageResponseTime.TotalMilliseconds:F2};");
        sb.AppendLine($"            const stdDev = {statistics.StandardDeviation:F2};");
        sb.AppendLine($"            const minTime = {statistics.MinResponseTime.TotalMilliseconds:F2};");
        sb.AppendLine($"            const maxTime = {statistics.MaxResponseTime.TotalMilliseconds:F2};");
        sb.AppendLine("            ");
        sb.AppendLine("            // Create range for bell curve (3 standard deviations on each side)");
        sb.AppendLine("            const rangeStart = Math.max(0, mean - 3 * stdDev);");
        sb.AppendLine("            const rangeEnd = mean + 3 * stdDev;");
        sb.AppendLine("            const step = (rangeEnd - rangeStart) / 100;");
        sb.AppendLine("            ");
        sb.AppendLine("            const normalDistributionData = [];");
        sb.AppendLine("            const normalDistributionLabels = [];");
        sb.AppendLine("            ");
        sb.AppendLine("            // Generate normal distribution curve");
        sb.AppendLine("            for (let x = rangeStart; x <= rangeEnd; x += step) {");
        sb.AppendLine("                const y = (1 / (stdDev * Math.sqrt(2 * Math.PI))) * Math.exp(-0.5 * Math.pow((x - mean) / stdDev, 2));");
        sb.AppendLine("                normalDistributionLabels.push(x.toFixed(0));");
        sb.AppendLine("                normalDistributionData.push(y);");
        sb.AppendLine("            }");
        sb.AppendLine("            ");
        sb.AppendLine("            // Create histogram of actual response times");
        sb.AppendLine("            const responseTimes = [");
        
        var successfulResponsesForChart = statistics.AllResponses.Where(r => r.IsSuccess).ToList();
        foreach (var response in successfulResponsesForChart)
        {
            sb.AppendLine($"                {response.ResponseTime.TotalMilliseconds:F0},");
        }
        
        sb.AppendLine("            ];");
        sb.AppendLine("            ");
        sb.AppendLine("            // Create histogram bins");
        sb.AppendLine("            const binCount = 20;");
        sb.AppendLine("            const binWidth = (maxTime - minTime) / binCount;");
        sb.AppendLine("            const histogramData = new Array(binCount).fill(0);");
        sb.AppendLine("            const histogramLabels = [];");
        sb.AppendLine("            ");
        sb.AppendLine("            for (let i = 0; i < binCount; i++) {");
        sb.AppendLine("                const binStart = minTime + i * binWidth;");
        sb.AppendLine("                histogramLabels.push(binStart.toFixed(0));");
        sb.AppendLine("            }");
        sb.AppendLine("            ");
        sb.AppendLine("            // Fill histogram bins");
        sb.AppendLine("            responseTimes.forEach(time => {");
        sb.AppendLine("                const binIndex = Math.min(Math.floor((time - minTime) / binWidth), binCount - 1);");
        sb.AppendLine("                histogramData[binIndex]++;");
        sb.AppendLine("            });");
        sb.AppendLine("            ");
        sb.AppendLine("            // Normalize histogram to match bell curve scale");
        sb.AppendLine("            const maxHistogramValue = Math.max(...histogramData);");
        sb.AppendLine("            const maxBellCurveValue = Math.max(...normalDistributionData);");
        sb.AppendLine("            const scaleFactor = maxBellCurveValue / maxHistogramValue * 0.8; // Scale to 80% of bell curve height");
        sb.AppendLine("            const normalizedHistogramData = histogramData.map(value => value * scaleFactor);");
        sb.AppendLine("            ");
        sb.AppendLine("            const normalChart = new Chart(normalCtx, {");
        sb.AppendLine("                type: 'line',");
        sb.AppendLine("                data: {");
        sb.AppendLine("                    labels: normalDistributionLabels,");
        sb.AppendLine("                    datasets: [{");
        sb.AppendLine("                        label: 'Normal Distribution (Theoretical)',");
        sb.AppendLine("                        data: normalDistributionData,");
        sb.AppendLine("                        borderColor: '#4a9eff',");
        sb.AppendLine("                        backgroundColor: 'rgba(74, 158, 255, 0.1)',");
        sb.AppendLine("                        fill: true,");
        sb.AppendLine("                        tension: 0.4,");
        sb.AppendLine("                        pointRadius: 0,");
        sb.AppendLine("                        borderWidth: 3");
        sb.AppendLine("                    }, {");
        sb.AppendLine("                        label: 'Mean (' + mean.toFixed(0) + 'ms)',");
        sb.AppendLine("                        data: normalDistributionLabels.map(x => {");
        sb.AppendLine("                            const xValue = parseFloat(x);");
        sb.AppendLine("                            return Math.abs(xValue - mean) < step ? Math.max(...normalDistributionData) : null;");
        sb.AppendLine("                        }),");
        sb.AppendLine("                        borderColor: '#dc3545',");
        sb.AppendLine("                        backgroundColor: 'transparent',");
        sb.AppendLine("                        borderWidth: 3,");
        sb.AppendLine("                        borderDash: [5, 5],");
        sb.AppendLine("                        pointRadius: 0,");
        sb.AppendLine("                        fill: false");
        sb.AppendLine("                    }, {");
        sb.AppendLine("                        label: '-1σ (' + (mean - stdDev).toFixed(0) + 'ms)',");
        sb.AppendLine("                        data: normalDistributionLabels.map(x => {");
        sb.AppendLine("                            const xValue = parseFloat(x);");
        sb.AppendLine("                            return Math.abs(xValue - (mean - stdDev)) < step ? Math.max(...normalDistributionData) * 0.8 : null;");
        sb.AppendLine("                        }),");
        sb.AppendLine("                        borderColor: '#ffc107',");
        sb.AppendLine("                        backgroundColor: 'transparent',");
        sb.AppendLine("                        borderWidth: 2,");
        sb.AppendLine("                        borderDash: [3, 3],");
        sb.AppendLine("                        pointRadius: 0,");
        sb.AppendLine("                        fill: false");
        sb.AppendLine("                    }, {");
        sb.AppendLine("                        label: '+1σ (' + (mean + stdDev).toFixed(0) + 'ms)',");
        sb.AppendLine("                        data: normalDistributionLabels.map(x => {");
        sb.AppendLine("                            const xValue = parseFloat(x);");
        sb.AppendLine("                            return Math.abs(xValue - (mean + stdDev)) < step ? Math.max(...normalDistributionData) * 0.8 : null;");
        sb.AppendLine("                        }),");
        sb.AppendLine("                        borderColor: '#ffc107',");
        sb.AppendLine("                        backgroundColor: 'transparent',");
        sb.AppendLine("                        borderWidth: 2,");
        sb.AppendLine("                        borderDash: [3, 3],");
        sb.AppendLine("                        pointRadius: 0,");
        sb.AppendLine("                        fill: false");
        sb.AppendLine("                    }]");
        sb.AppendLine("                },");
        sb.AppendLine("                options: {");
        sb.AppendLine("                    responsive: true,");
        sb.AppendLine("                    maintainAspectRatio: false,");
        sb.AppendLine("                    plugins: {");
        sb.AppendLine("                        legend: { labels: { color: '#b0b0b0' } }");
        sb.AppendLine("                    },");
        sb.AppendLine("                    scales: {");
        sb.AppendLine("                        y: {");
        sb.AppendLine("                            beginAtZero: true,");
        sb.AppendLine("                            title: {");
        sb.AppendLine("                                display: true,");
        sb.AppendLine("                                text: 'Probability Density',");
        sb.AppendLine("                                color: '#b0b0b0'");
        sb.AppendLine("                            },");
        sb.AppendLine("                            ticks: { color: '#b0b0b0' },");
        sb.AppendLine("                            grid: { color: '#555555' }");
        sb.AppendLine("                        },");
        sb.AppendLine("                        x: {");
        sb.AppendLine("                            title: {");
        sb.AppendLine("                                display: true,");
        sb.AppendLine("                                text: 'Response Time (ms)',");
        sb.AppendLine("                                color: '#b0b0b0'");
        sb.AppendLine("                            },");
        sb.AppendLine("                            ticks: { color: '#b0b0b0' },");
        sb.AppendLine("                            grid: { color: '#555555' }");
        sb.AppendLine("                        }");
        sb.AppendLine("                    }");
        sb.AppendLine("                }");
        sb.AppendLine("            });");
        sb.AppendLine("        </script>");
        
        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }
}
