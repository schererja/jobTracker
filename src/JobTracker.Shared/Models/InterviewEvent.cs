using System.Text.Json.Serialization;

namespace JobTracker.Shared.Models;

/// <summary>
/// Represents an interview or assessment event for a job application.
/// </summary>
public class InterviewEvent
{
  /// <summary>
  /// Unique identifier for the interview event (required by Cosmos DB).
  /// </summary>
  [JsonPropertyName("id")]
  public string id { get; set; } = string.Empty;

  /// <summary>
  /// Application ID for partition key (required by Cosmos DB).
  /// </summary>
  [JsonPropertyName("applicationId")]
  public string applicationId { get; set; } = string.Empty;

  /// <summary>
  /// Unique identifier for the interview event (internal use).
  /// </summary>
  [JsonIgnore]
  public Guid InterviewId
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
  /// Scheduled date and time of the interview.
  /// </summary>
  public DateTime Date { get; set; }

  /// <summary>
  /// Name(s) of the interviewer(s).
  /// </summary>
  public string? Interviewer { get; set; }

  /// <summary>
  /// Type of interview.
  /// </summary>
  public InterviewType Type { get; set; }

  /// <summary>
  /// Notes about the interview (preparation, follow-up, feedback, etc.).
  /// </summary>
  public string? Notes { get; set; }

  /// <summary>
  /// Timestamp when this record was created.
  /// </summary>
  public DateTime CreatedAt { get; set; }

  /// <summary>
  /// Timestamp when this record was last updated.
  /// </summary>
  public DateTime UpdatedAt { get; set; }
}
