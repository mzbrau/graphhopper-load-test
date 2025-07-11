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
        sb.AppendLine("        </script>");
        
        sb.AppendLine("    </div>");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }
}
