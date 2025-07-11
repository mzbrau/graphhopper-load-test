using Cocona;
using GraphhopperLoadTest.Commands;
using GraphhopperLoadTest.Configuration;

var builder = CoconaApp.CreateBuilder();

// Register all services
builder.Services.AddLoadTestServices();

var app = builder.Build();

// Register command handlers
app.AddCommands<LoadTestCommand>();

await app.RunAsync();