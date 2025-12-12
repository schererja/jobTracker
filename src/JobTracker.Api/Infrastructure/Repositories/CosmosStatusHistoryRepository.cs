using Microsoft.Azure.Cosmos;
using JobTracker.Shared.Models;

namespace JobTracker.Api.Infrastructure.Repositories;

/// <summary>
/// Cosmos DB implementation of IStatusHistoryRepository.
/// </summary>
public class CosmosStatusHistoryRepository : IStatusHistoryRepository
{
  private readonly Container _container;

  public CosmosStatusHistoryRepository(Container container)
  {
    _container = container;
  }

  public async Task<StatusHistory> AppendAsync(StatusHistory history, CancellationToken ct = default)
  {
    var response = await _container.CreateItemAsync(history, cancellationToken: ct);
    return response.Resource;
  }

  public async Task<(List<StatusHistory> items, string? continuationToken)> ListAsync(
      Guid applicationId,
      int pageSize = 50,
      string? continuationToken = null,
      CancellationToken ct = default)
  {
    var query = _container.GetItemQueryIterator<StatusHistory>(
        new QueryDefinition("SELECT * FROM c WHERE c.applicationId = @appId ORDER BY c.changedAt DESC")
            .WithParameter("@appId", applicationId.ToString()),
        requestOptions: new QueryRequestOptions
        {
          MaxItemCount = pageSize,
          PartitionKey = new PartitionKey(applicationId.ToString())
        });

    if (!string.IsNullOrEmpty(continuationToken))
    {
      query = _container.GetItemQueryIterator<StatusHistory>(
          new QueryDefinition("SELECT * FROM c WHERE c.applicationId = @appId ORDER BY c.changedAt DESC")
              .WithParameter("@appId", applicationId.ToString()),
          continuationToken: continuationToken,
          requestOptions: new QueryRequestOptions
          {
            MaxItemCount = pageSize,
            PartitionKey = new PartitionKey(applicationId.ToString())
          });
    }

    var batch = await query.ReadNextAsync(ct);
    return (batch.ToList(), query.HasMoreResults ? batch.ContinuationToken : null);
  }
}
