<div align="center">

# 🚀 GraphHopper Load Test

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/mzbrau/graphhopper-load-test?style=for-the-badge)](LICENSE)
[![Release](https://img.shields.io/github/v/release/mzbrau/graphhopper-load-test?style=for-the-badge)](https://github.com/mzbrau/graphhopper-load-test/releases)
[![Downloads](https://img.shields.io/github/downloads/mzbrau/graphhopper-load-test/total?style=for-the-badge)](https://github.com/mzbrau/graphhopper-load-test/releases)

**A comprehensive load testing tool for GraphHopper routing services**

*Built with C# and .NET 9 for high-performance routing API testing*

[📥 Download](#-download) • [🚀 Quick Start](#-quick-start) • [📖 Documentation](#-usage) • [🤝 Contributing](#-contributing)

</div>

---

## ✨ Features

<table>
<tr>
<td width="50%">

### 🔄 Progressive Load Testing
- Starts with 1 thread, adds new threads incrementally
- Configurable thread start intervals
- Realistic load progression simulation

### 🎯 Intelligent Route Generation
- Random target locations within configurable radius
- Dynamic source location generation (40-50km from target)
- Realistic geographic distribution

</td>
<td width="50%">

### 📊 Comprehensive Reporting
- **Dark-themed HTML reports** with professional styling
- Real-time response time visualization
- **Requests over time graph** showing load progression
- Complete statistical analysis

### ⚙️ Flexible Configuration
- Full command-line interface with sensible defaults
- Configurable test parameters
- Support for custom test names and timestamps

</td>
</tr>
</table>

## 📋 Prerequisites

<table>
<tr>
<td align="center">
<img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=flat&logo=dotnet" alt=".NET 9.0"/>
<br><strong>.NET 9.0 SDK</strong>
</td>
<td align="center">
<img src="https://img.shields.io/badge/GraphHopper-Server-4CAF50?style=flat" alt="GraphHopper"/>
<br><strong>GraphHopper Server</strong>
</td>
</tr>
</table>

## 📥 Download

### Pre-built Binaries (Recommended)

Download the latest release for your platform from [GitHub Releases](https://github.com/mzbrau/graphhopper-load-test/releases):

| Platform | Download | Requirements |
|----------|----------|--------------|
| 🪟 **Windows (x64)** | `graphhopper-load-test-{version}-win-x64.exe` | None - Self-contained |
| 🐧 **Linux (x64)** | `graphhopper-load-test-{version}-linux-x64` | None - Self-contained |
| 🍎 **macOS (Intel)** | `graphhopper-load-test-{version}-osx-x64` | None - Self-contained |
| 🍎 **macOS (Apple Silicon)** | `graphhopper-load-test-{version}-osx-arm64` | None - Self-contained |

### Build from Source

```bash
# Clone the repository
git clone https://github.com/mzbrau/graphhopper-load-test.git
cd graphhopper-load-test

# Build the project
cd src/GraphhopperLoadTest
dotnet build

# Run directly
dotnet run
```

## 🚀 Quick Start

### Using Pre-built Binary

```bash
# Download and run (example for Linux)
wget https://github.com/mzbrau/graphhopper-load-test/releases/latest/download/graphhopper-load-test-linux-x64
chmod +x graphhopper-load-test-linux-x64
./graphhopper-load-test-linux-x64
```

### Using .NET CLI

```bash
# Navigate to project directory
cd src/GraphhopperLoadTest/GraphhopperLoadTest

# Run with default settings (London, localhost GraphHopper)
dotnet run

# Run with custom test name and shorter duration
dotnet run -- --test-name "My Load Test" --duration 5
```

## 📖 Usage

### Command Line Options

| Option | Short | Description | Default | Example |
|--------|-------|-------------|---------|---------|
| `--url` | `-u` | GraphHopper server URL | `http://localhost:8989` | `https://api.example.com` |
| `--center-point` | `-c` | Center point (lat,lng) | `51.5074,-0.1278` (London) | `40.7128,-74.0060` (NYC) |
| `--duration` | `-d` | Test duration (minutes) | `10` | `5` |
| `--thread-interval` | `-i` | Thread start interval (minutes) | `1` | `2` |
| `--request-delay` | `-r` | Request delay (milliseconds) | `1000` | `500` |
| `--target-radius` | `-t` | Target radius (km) | `5.0` | `10.0` |
| `--source-radius-min` | `-s` | Min source radius (km) | `40.0` | `30.0` |
| `--source-radius-max` | `-S` | Max source radius (km) | `50.0` | `60.0` |
| `--output` | `-o` | Output HTML file path | Auto-timestamped | `my-test.html` |
| `--test-name` | `-n` | Test name for report | None | `"Performance Test v1.0"` |
| `--no-instructions` | | Disable route instructions | `false` | |
| `--verbose` | `-v` | Enable verbose logging | `false` | |

### 💡 Usage Examples

<details>
<summary><strong>🌍 Test Remote GraphHopper Server</strong></summary>

```bash
dotnet run -- --url "https://graphhopper.example.com" \
              --center-point "40.7128,-74.0060" \
              --test-name "NYC Production Test"
```

</details>

<details>
<summary><strong>⚡ High-Intensity Load Test</strong></summary>

```bash
dotnet run -- --thread-interval 1 \
              --request-delay 100 \
              --duration 10 \
              --test-name "High Load Stress Test"
```

</details>

<details>
<summary><strong>🎯 Custom Location Testing (London)</strong></summary>

```bash
dotnet run -- --center-point "51.5074,-0.1278" \
              --target-radius 8.0 \
              --test-name "London Metropolitan Test" \
              --verbose
```

</details>

<details>
<summary><strong>🚀 Quick Performance Check</strong></summary>

```bash
dotnet run -- --duration 2 \
              --request-delay 500 \
              --no-instructions \
              --test-name "Quick Smoke Test"
```

</details>

## 📊 Report Output

The tool generates timestamped HTML reports with a **modern dark theme** and comprehensive analytics:

### 📈 Visual Analytics
- **Request Volume Over Time**: Shows load progression as threads are added
- **Response Time Trends**: Real-time performance monitoring
- **Success Rate Tracking**: Visual success/failure ratios

### 📋 Detailed Metrics
- Complete statistical analysis (min, max, avg, std dev)
- Per-thread performance breakdown
- Timestamp-sorted request logs
- Error categorization and reporting

### 🎨 Professional Styling
- Dark theme optimized for extended viewing
- Responsive design for all screen sizes
- Interactive charts with hover details
- Professional color scheme and typography

## 🔄 Test Behavior

| Phase | Description | Behavior |
|-------|-------------|----------|
| **🚀 Initialization** | Test setup and validation | Validates parameters, tests connectivity |
| **📈 Load Ramp-up** | Progressive thread addition | Starts with 1 thread, adds 1 every interval |
| **🎯 Request Generation** | Continuous API calls | Each thread makes requests with specified delay |
| **📊 Data Collection** | Real-time monitoring | Tracks response times, success rates, errors |
| **📝 Report Generation** | Final analysis | Creates comprehensive HTML report |

### Geographic Distribution
- **Target Selection**: Random locations within radius of center point
- **Source Generation**: Dynamic locations 40-50km from each target
- **Realistic Patterns**: Simulates real-world routing requests

## 🏗️ Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│  CLI Interface  │───▶│  Load Test Runner │───▶│  Report Generator│
│   (Cocona)      │    │                  │    │                 │
└─────────────────┘    └──────────────────┘    └─────────────────┘
                                │
                    ┌───────────▼───────────┐
                    │    Service Layer      │
                    │                       │
                    │ • Coordinate Generator│
                    │ • GraphHopper Client  │
                    │ • Statistics Tracker │
                    └───────────────────────┘
```

### Core Components

| Component | Responsibility | Key Features |
|-----------|----------------|--------------|
| **🎯 LoadTestRunner** | Orchestrates test execution | Thread management, progress tracking |
| **🌐 GraphhopperClient** | HTTP communication | Request/response handling, error management |
| **📍 CoordinateGenerator** | Geographic calculations | Random coordinate generation within radii |
| **📊 ReportGenerator** | Output generation | HTML reports with charts and analytics |

## 🚢 Building & Publishing

### Development Build

```bash
cd src/GraphhopperLoadTest/GraphhopperLoadTest
dotnet build --configuration Debug
```

### Production Release

```bash
# Self-contained single file for current platform
dotnet publish --configuration Release --self-contained true -p:PublishSingleFile=true

# Cross-platform builds
dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true
dotnet publish -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true
dotnet publish -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true
```

### 🏷️ Versioning

This project uses [MinVer](https://github.com/adamralph/minver) for automatic semantic versioning:

- **Development**: `1.0.0-alpha.0+{commit-hash}`
- **Releases**: `{tag-version}` (e.g., `1.2.3` for tag `v1.2.3`)

```bash
# Create and push a release tag
git tag v1.0.0
git push origin v1.0.0
```

## 🛠️ Error Handling & Reliability

| Error Type | Handling Strategy | Impact |
|------------|------------------|--------|
| **🔌 Connection Failures** | Logged and tracked as failures | Test continues with remaining threads |
| **⚠️ Invalid Responses** | Categorized and counted separately | Detailed error reporting |
| **🛑 Early Termination** | Graceful shutdown with Ctrl+C | Partial results saved |
| **📊 Report Errors** | Fallback to basic statistics | Ensures data preservation |

## 🤝 Contributing

We welcome contributions! Here's how to get started:

<table>
<tr>
<td align="center">

### 🍴 Fork & Clone
```bash
git clone https://github.com/yourusername/graphhopper-load-test.git
```

</td>
<td align="center">

### 🌿 Create Branch
```bash
git checkout -b feature/amazing-feature
```

</td>
<td align="center">

### 🚀 Submit PR
Open a pull request with detailed description

</td>
</tr>
</table>

### Development Guidelines

- **📝 Code Style**: Follow existing patterns and C# conventions
- **🏗️ Architecture**: Maintain SOLID principles and clean architecture
- **✅ Testing**: Add tests for new functionality
- **📖 Documentation**: Update README and code comments
- **🔄 Reviews**: All PRs require review before merging

## 📜 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

<div align="center">

### ⭐ Star this repository if it helped you!

**Made with ❤️ by the GraphHopper Load Test Team**

[Report Issues](https://github.com/mzbrau/graphhopper-load-test/issues) • [Request Features](https://github.com/mzbrau/graphhopper-load-test/issues/new?template=feature_request.md) • [View Releases](https://github.com/mzbrau/graphhopper-load-test/releases)

</div>
