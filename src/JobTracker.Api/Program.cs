using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Cosmos;
using JobTracker.Api.Infrastructure.Repositories;
using JobTracker.Api.Infrastructure.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add Application Insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Add HttpContextAccessor for identity service
builder.Services.AddHttpContextAccessor();

// Add Identity Service
builder.Services.AddScoped<IIdentityService, HttpContextIdentityService>();

// Add Storage Service (mock for development, replace with Azure Blob Storage implementation)
builder.Services.AddScoped<IStorageService, MockStorageService>();

// Configure Cosmos DB
var cosmosConnectionString = builder.Configuration["CosmosDbConnectionString"]
    ?? "DefaultEndpointsProtocol=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPDAe8F7gIJlJ0/7mB+sjeIQg==;";
var cosmosDatabaseName = builder.Configuration["CosmosDbDatabaseName"] ?? "jobtracker";
var cosmosContainerName = builder.Configuration["CosmosDbContainerName"] ?? "items";

var cosmosClient = new CosmosClient(cosmosConnectionString);
var database = cosmosClient.GetDatabase(cosmosDatabaseName);
var container = database.GetContainer(cosmosContainerName);

// Register repositories as singletons (Container is thread-safe)
builder.Services.AddSingleton<IApplicationRepository>(sp => new CosmosApplicationRepository(container));
builder.Services.AddSingleton<IInterviewRepository>(sp => new CosmosInterviewRepository(container));
builder.Services.AddSingleton<IStatusHistoryRepository>(sp => new CosmosStatusHistoryRepository(container));
builder.Services.AddSingleton<IAttachmentRepository>(sp => new CosmosAttachmentRepository(container));
builder.Services.AddSingleton<IUserRepository>(sp => new CosmosUserRepository(container));

var app = builder.Build();

app.Run();
