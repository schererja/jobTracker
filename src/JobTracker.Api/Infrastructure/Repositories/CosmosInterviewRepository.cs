using Microsoft.Azure.Cosmos;
using JobTracker.Shared.Models;

namespace JobTracker.Api.Infrastructure.Repositories;

/// <summary>
/// Cosmos DB implementation of IInterviewRepository.
/// </summary>
public class CosmosInterviewRepository : IInterviewRepository
{
  private readonly Container _container;

  public CosmosInterviewRepository(Container container)
  {
    _container = container;
  }

  public async Task<InterviewEvent> CreateAsync(InterviewEvent interview, CancellationToken ct = default)
  {
    interview.CreatedAt = DateTime.UtcNow;
    interview.UpdatedAt = DateTime.UtcNow;

    var response = await _container.CreateItemAsync(interview, cancellationToken: ct);
    return response.Resource;
  }

  public async Task<InterviewEvent?> GetByIdAsync(Guid interviewId, Guid applicationId, CancellationToken ct = default)
  {
    try
    {
      var response = await _container.ReadItemAsync<InterviewEvent>(
          interviewId.ToString(),
          new PartitionKey(applicationId.ToString()),
          cancellationToken: ct);
      return response.Resource;
    }
    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
      return null;
    }
  }

  public async Task<(List<InterviewEvent> items, string? continuationToken)> ListAsync(
      Guid applicationId,
      int pageSize = 50,
      string? continuationToken = null,
      CancellationToken ct = default)
  {
    var query = _container.GetItemQueryIterator<InterviewEvent>(
        new QueryDefinition("SELECT * FROM c WHERE c.applicationId = @appId ORDER BY c.date DESC")
            .WithParameter("@appId", applicationId.ToString()),
            requestOptions: new QueryRequestOptions { MaxItemCount = pageSize, PartitionKey = new PartitionKey(applicationId.ToString()) });

    if (!string.IsNullOrEmpty(continuationToken))
    {
      query = _container.GetItemQueryIterator<InterviewEvent>(
          new QueryDefinition("SELECT * FROM c WHERE c.applicationId = @appId ORDER BY c.date DESC")
              .WithParameter("@appId", applicationId.ToString()),
          continuationToken: continuationToken,
          requestOptions: new QueryRequestOptions { MaxItemCount = pageSize, PartitionKey = new PartitionKey(applicationId.ToString()) });
    }

    var batch = await query.ReadNextAsync(ct);
    return (batch.ToList(), query.HasMoreResults ? batch.ContinuationToken : null);
  }

  public async Task<InterviewEvent> UpdateAsync(InterviewEvent interview, CancellationToken ct = default)
  {
    interview.UpdatedAt = DateTime.UtcNow;

    var response = await _container.ReplaceItemAsync(
        interview,
        interview.InterviewId.ToString(),
        new PartitionKey(interview.ApplicationId.ToString()),
        cancellationToken: ct);
    return response.Resource;
  }

  public async Task DeleteAsync(Guid interviewId, Guid applicationId, CancellationToken ct = default)
  {
    await _container.DeleteItemAsync<InterviewEvent>(
        interviewId.ToString(),
        new PartitionKey(applicationId.ToString()),
        cancellationToken: ct);
  }
}
