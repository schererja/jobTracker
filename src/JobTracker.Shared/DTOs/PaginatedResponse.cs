namespace JobTracker.Shared.DTOs;

/// <summary>
/// Generic paginated response wrapper.
/// </summary>
public class PaginatedResponse<T>
{
  public List<T> Items { get; set; } = new();
  public string? ContinuationToken { get; set; }
  public int Count { get; set; }
}
