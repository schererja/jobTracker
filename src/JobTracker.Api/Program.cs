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

// Add JWT Token Service (needed for both dev and prod)
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Add Identity Service and Storage Service based on environment
if (builder.Environment.IsDevelopment())
{
  // Use mock services for local development (no auth required)
  builder.Services.AddScoped<IIdentityService, MockIdentityService>();
  builder.Services.AddScoped<IStorageService, MockStorageService>();
}
else
{
  // Use real services for production (JWT auth required)
  builder.Services.AddScoped<IIdentityService, JwtIdentityService>();
  // TODO: Replace with real Azure Blob Storage service when ready
  builder.Services.AddScoped<IStorageService, MockStorageService>();
}

// Configure Cosmos DB
var cosmosConnectionString = builder.Configuration["CosmosDbConnectionString"]
    ?? "DefaultEndpointsProtocol=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPDAe8F7gIJlJ0/7mB+sjeIQg==;";
var cosmosDatabaseName = builder.Configuration["CosmosDbDatabaseName"] ?? "jobtracker";
var cosmosContainerName = builder.Configuration["CosmosDbContainerName"] ?? "items";

// Configure Cosmos with System.Text.Json serializer for camelCase
var cosmosOptions = new CosmosClientOptions
{
  SerializerOptions = new CosmosSerializationOptions
  {
    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
  }
};

var cosmosClient = new CosmosClient(cosmosConnectionString, cosmosOptions);
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
