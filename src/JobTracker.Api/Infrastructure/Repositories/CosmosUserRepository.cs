using Microsoft.Azure.Cosmos;
using User = JobTracker.Shared.Models.User;

namespace JobTracker.Api.Infrastructure.Repositories;

/// <summary>
/// Cosmos DB implementation of IUserRepository.
/// Uses partition key: /userId
/// </summary>
public class CosmosUserRepository : IUserRepository
{
  private readonly Container _container;

  public CosmosUserRepository(Container container)
  {
    _container = container;
  }

  public async Task<User> GetOrCreateAsync(string email, CancellationToken ct = default)
  {
    var existing = await GetByEmailAsync(email, ct);
    if (existing != null)
      return existing;

    var userId = Guid.NewGuid().ToString();
    var newUser = new User
    {
      Id = userId,
      UserId = userId,
      Email = email,
      CreatedAt = DateTime.UtcNow,
      Plan = "free"
    };

    var response = await _container.CreateItemAsync(newUser, new PartitionKey(userId), cancellationToken: ct);
    return response.Resource;
  }

  public async Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default)
  {
    try
    {
      var response = await _container.ReadItemAsync<User>(
          userId.ToString(),
          new PartitionKey(userId.ToString()),
          cancellationToken: ct);
      return response.Resource;
    }
    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
      return null;
    }
  }

  public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
  {
    var query = _container.GetItemQueryIterator<User>(
        new QueryDefinition("SELECT * FROM c WHERE LOWER(c.email) = @email")
            .WithParameter("@email", email.ToLower()));

    var batch = await query.ReadNextAsync(ct);
    return batch.FirstOrDefault();
  }

  public async Task<User> CreateAsync(User user, CancellationToken ct = default)
  {
    var response = await _container.CreateItemAsync(user, new PartitionKey(user.UserId), cancellationToken: ct);
    return response.Resource;
  }

  public async Task<User> UpdateAsync(User user, CancellationToken ct = default)
  {
    var response = await _container.ReplaceItemAsync(
        user,
        user.UserId.ToString(),
        new PartitionKey(user.UserId.ToString()),
        cancellationToken: ct);
    return response.Resource;
  }
}
