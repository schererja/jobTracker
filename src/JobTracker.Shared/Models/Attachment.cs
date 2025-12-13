using System.Text.Json.Serialization;

namespace JobTracker.Shared.Models;

/// <summary>
/// Represents a file attachment associated with a job application.
/// </summary>
public class Attachment
{
  /// <summary>
  /// Unique identifier for the attachment (required by Cosmos DB).
  /// </summary>
  [JsonPropertyName("id")]
  public string id { get; set; } = string.Empty;

  /// <summary>
  /// Application ID for partition key (required by Cosmos DB).
  /// </summary>
  [JsonPropertyName("applicationId")]
  public string applicationId { get; set; } = string.Empty;

  /// <summary>
  /// Unique identifier for the attachment (internal use).
  /// </summary>
  [JsonIgnore]
  public Guid AttachmentId
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
  /// Type of attachment (e.g., "Resume", "CoverLetter", "Email", "Screenshot").
  /// </summary>
  public string Type { get; set; } = string.Empty;

  /// <summary>
  /// Original filename.
  /// </summary>
  public string FileName { get; set; } = string.Empty;

  /// <summary>
  /// Content type (MIME type) of the file.
  /// </summary>
  public string ContentType { get; set; } = string.Empty;

  /// <summary>
  /// File size in bytes.
  /// </summary>
  public long SizeBytes { get; set; }

  /// <summary>
  /// Storage path or key in S3/Blob Storage.
  /// </summary>
  public string StoragePath { get; set; } = string.Empty;

  /// <summary>
  /// Timestamp when the attachment was uploaded.
  /// </summary>
  public DateTime UploadedAt { get; set; }
}
