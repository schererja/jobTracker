namespace JobTracker.Shared.Models;

/// <summary>
/// Represents the current status of a job application.
/// </summary>
public enum ApplicationStatus
{
  Applied,
  Interviewing,
  Offer,
  Rejected,
  Ghosted,
  Withdrawn
}
