using Microsoft.Azure.Cosmos;
using JobTracker.Shared.Models;
using JobTracker.Shared.DTOs;

namespace JobTracker.Api.Infrastructure.Repositories;

/// <summary>
/// Cosmos DB implementation of IApplicationRepository.
/// Uses partition key: /userId
/// </summary>
public class CosmosApplicationRepository : IApplicationRepository
{
  private readonly Container _container;

  public CosmosApplicationRepository(Container container)
  {
    _container = container;
  }

  public async Task<JobApplication> CreateAsync(JobApplication application, CancellationToken ct = default)
  {
    application.CreatedAt = DateTime.UtcNow;
    application.UpdatedAt = DateTime.UtcNow;

    var response = await _container.CreateItemAsync(application, new PartitionKey(application.userId), cancellationToken: ct);
    return response.Resource;
  }

  public async Task<JobApplication?> GetByIdAsync(Guid applicationId, Guid userId, CancellationToken ct = default)
  {
    try
    {
      var response = await _container.ReadItemAsync<JobApplication>(
          applicationId.ToString(),
          new PartitionKey(userId.ToString()),
          cancellationToken: ct);
      return response.Resource;
    }
    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
      return null;
    }
  }

  public async Task<PaginatedResponse<JobApplication>> ListAsync(
      Guid userId,
      ApplicationStatus[]? statuses = null,
      string? company = null,
      ApplicationSource? source = null,
      DateTime? appliedFrom = null,
      DateTime? appliedTo = null,
      string? searchQuery = null,
      int pageSize = 20,
      string? continuationToken = null,
      CancellationToken ct = default)
  {
    FeedIterator<JobApplication> query;

    if (!string.IsNullOrEmpty(continuationToken))
    {
      query = _container.GetItemQueryIterator<JobApplication>(
          BuildQueryDefinition(userId, statuses, company, source, appliedFrom, appliedTo, searchQuery),
          continuationToken: continuationToken,
          requestOptions: new QueryRequestOptions
          {
            MaxItemCount = pageSize,
            PartitionKey = new PartitionKey(userId.ToString())
          });
    }
    else
    {
      query = _container.GetItemQueryIterator<JobApplication>(
          BuildQueryDefinition(userId, statuses, company, source, appliedFrom, appliedTo, searchQuery),
          requestOptions: new QueryRequestOptions
          {
            MaxItemCount = pageSize,
            PartitionKey = new PartitionKey(userId.ToString())
          });
    }

    var batch = await query.ReadNextAsync(ct);
    var items = batch.ToList();
    var nextToken = query.HasMoreResults ? batch.ContinuationToken : null;

    return new PaginatedResponse<JobApplication>
    {
      Items = items,
      ContinuationToken = nextToken
    };
  }

  public async Task<JobApplication> UpdateAsync(JobApplication application, CancellationToken ct = default)
  {
    application.UpdatedAt = DateTime.UtcNow;

    var response = await _container.ReplaceItemAsync(
        application,
        application.id,
        new PartitionKey(application.userId),
        cancellationToken: ct);
    return response.Resource;
  }

  public async Task DeleteAsync(Guid applicationId, Guid userId, CancellationToken ct = default)
  {
    await _container.DeleteItemAsync<JobApplication>(
        applicationId.ToString(),
        new PartitionKey(userId.ToString()),
        cancellationToken: ct);
  }

  public async Task<int> CountAsync(Guid userId, CancellationToken ct = default)
  {
    var query = _container.GetItemQueryIterator<dynamic>(
        new QueryDefinition("SELECT VALUE COUNT(1) FROM c WHERE c.userId = @userId")
            .WithParameter("@userId", userId.ToString()),
        requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(userId.ToString()) });

    var batch = await query.ReadNextAsync(ct);
    return batch.First();
  }

  private QueryDefinition BuildQueryDefinition(
      Guid userId,
      ApplicationStatus[]? statuses,
      string? company,
      ApplicationSource? source,
      DateTime? appliedFrom,
      DateTime? appliedTo,
      string? searchQuery)
  {
    var queryText = "SELECT * FROM c WHERE c.userId = @userId";

    // Build query text
    var parameters = new Dictionary<string, object> { ["@userId"] = userId.ToString() };

    if (statuses?.Length > 0)
    {
      var statusArray = string.Join(",", statuses.Select((_, i) => $"@status{i}"));
      queryText += $" AND c.status IN ({statusArray})";
      for (int i = 0; i < statuses.Length; i++)
      {
        parameters[$"@status{i}"] = (int)statuses[i];
      }
    }

    if (!string.IsNullOrEmpty(company))
    {
      queryText += " AND CONTAINS(LOWER(c.company), @company)";
      parameters["@company"] = company.ToLower();
    }

    if (source.HasValue)
    {
      queryText += " AND c.source = @source";
      parameters["@source"] = (int)source.Value;
    }

    if (appliedFrom.HasValue)
    {
      queryText += " AND c.appliedDate >= @appliedFrom";
      parameters["@appliedFrom"] = appliedFrom.Value;
    }

    if (appliedTo.HasValue)
    {
      queryText += " AND c.appliedDate <= @appliedTo";
      parameters["@appliedTo"] = appliedTo.Value;
    }

    if (!string.IsNullOrEmpty(searchQuery))
    {
      queryText += " AND (CONTAINS(LOWER(c.company), @search) OR CONTAINS(LOWER(c.roleTitle), @search))";
      parameters["@search"] = searchQuery.ToLower();
    }

    queryText += " ORDER BY c.appliedDate DESC";

    // Build the query with all parameters
    var query = new QueryDefinition(queryText);
    foreach (var param in parameters)
    {
      query = query.WithParameter(param.Key, param.Value);
    }

    return query;
  }
}

