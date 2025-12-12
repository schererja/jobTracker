using Microsoft.Azure.Cosmos;
using JobTracker.Shared.Models;

namespace JobTracker.Api.Infrastructure.Repositories;

/// <summary>
/// Cosmos DB implementation of IAttachmentRepository.
/// </summary>
public class CosmosAttachmentRepository : IAttachmentRepository
{
  private readonly Container _container;

  public CosmosAttachmentRepository(Container container)
  {
    _container = container;
  }

  public async Task<Attachment> CreateAsync(Attachment attachment, CancellationToken ct = default)
  {
    var response = await _container.CreateItemAsync(attachment, cancellationToken: ct);
    return response.Resource;
  }

  public async Task<Attachment?> GetByIdAsync(Guid attachmentId, Guid applicationId, CancellationToken ct = default)
  {
    try
    {
      var response = await _container.ReadItemAsync<Attachment>(
          attachmentId.ToString(),
          new PartitionKey(applicationId.ToString()),
          cancellationToken: ct);
      return response.Resource;
    }
    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
      return null;
    }
  }

  public async Task<(List<Attachment> items, string? continuationToken)> ListAsync(
      Guid applicationId,
      int pageSize = 50,
      string? continuationToken = null,
      CancellationToken ct = default)
  {
    var query = _container.GetItemQueryIterator<Attachment>(
        new QueryDefinition("SELECT * FROM c WHERE c.applicationId = @appId ORDER BY c.uploadedAt DESC")
            .WithParameter("@appId", applicationId.ToString()),
        requestOptions: new QueryRequestOptions
        {
          MaxItemCount = pageSize,
          PartitionKey = new PartitionKey(applicationId.ToString())
        });

    if (!string.IsNullOrEmpty(continuationToken))
    {
      query = _container.GetItemQueryIterator<Attachment>(
          new QueryDefinition("SELECT * FROM c WHERE c.applicationId = @appId ORDER BY c.uploadedAt DESC")
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
  public async Task DeleteAsync(Guid attachmentId, Guid applicationId, CancellationToken ct = default)
  {
    await _container.DeleteItemAsync<Attachment>(
        attachmentId.ToString(),
        new PartitionKey(applicationId.ToString()),
        cancellationToken: ct);
  }
}