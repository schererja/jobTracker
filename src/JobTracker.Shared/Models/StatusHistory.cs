using System.Text.Json.Serialization;

namespace JobTracker.Shared.Models;

/// <summary>
/// Represents a status change event for a job application (audit trail).
/// </summary>
public class StatusHistory
{
  /// <summary>
  /// Unique identifier for the status history entry (required by Cosmos DB).
  /// </summary>
  [JsonPropertyName("id")]
  public string id { get; set; } = string.Empty;

  /// <summary>
  /// Application ID for partition key (required by Cosmos DB).
  /// </summary>
  [JsonPropertyName("applicationId")]
  public string applicationId { get; set; } = string.Empty;

  /// <summary>
  /// Unique identifier for the status history entry (internal use).
  /// </summary>
  [JsonIgnore]
  public Guid HistoryId
  {
    get => Guid.TryParse(id, out var g) ? g : Guid.Empty;
    set => id = value.ToString();
  }

  /// <summary>
  /// ID of the associated job application (internal use).
  /// </summary>
  [JsonIgnore]
  public Guid ApplicationId
  {
    get => Guid.TryParse(applicationId, out var g) ? g : Guid.Empty;
    set => applicationId = value.ToString();
  }

  /// <summary>
  /// Previous status before the change.
  /// </summary>
  public ApplicationStatus OldStatus { get; set; }

  /// <summary>
  /// New status after the change.
  /// </summary>
  public ApplicationStatus NewStatus { get; set; }

  /// <summary>
  /// Timestamp when the status change occurred.
  /// </summary>
  public DateTime ChangedAt { get; set; }
}
