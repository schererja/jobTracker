using System.Text.Json.Serialization;

namespace JobTracker.Shared.Models;

/// <summary>
/// Represents a job application submission.
/// </summary>
public class JobApplication
{
  /// <summary>
  /// Unique identifier for the job application (required by Cosmos DB).
  /// </summary>
  public string id { get; set; } = string.Empty;

  /// <summary>
  /// User ID for partition key (required by Cosmos DB).
  /// </summary>
  public string userId { get; set; } = string.Empty;

  /// <summary>
  /// Company name.
  /// </summary>
  public string Company { get; set; } = string.Empty;

  /// <summary>
  /// Job role/title.
  /// </summary>
  public string RoleTitle { get; set; } = string.Empty;

  /// <summary>
  /// Job location (e.g., "Remote", "New York, NY", "Hybrid - Seattle").
  /// </summary>
  public string? Location { get; set; }

  /// <summary>
  /// Salary range (e.g., "$120k-$150k", "Negotiable").
  /// </summary>
  public string? SalaryRange { get; set; }

  /// <summary>
  /// Date when the application was submitted.
  /// </summary>
  public DateTime AppliedDate { get; set; }

  /// <summary>
  /// Current status of the application.
  /// </summary>
  public ApplicationStatus Status { get; set; }

  /// <summary>
  /// Source where the job was found and applied.
  /// </summary>
  public ApplicationSource Source { get; set; }

  /// <summary>
  /// URL to the job posting or application portal.
  /// </summary>
  public string? Url { get; set; }

  /// <summary>
  /// Which resume version was used for this application.
  /// </summary>
  public string? ResumeUsed { get; set; }

  /// <summary>
  /// Free-form notes about the application.
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
