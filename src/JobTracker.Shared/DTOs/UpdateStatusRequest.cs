using JobTracker.Shared.Models;

namespace JobTracker.Shared.DTOs;

/// <summary>
/// Request to update the status of a job application.
/// </summary>
public class UpdateStatusRequest
{
  public ApplicationStatus NewStatus { get; set; }
}
