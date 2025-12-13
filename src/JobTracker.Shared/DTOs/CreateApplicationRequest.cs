using JobTracker.Shared.Models;

namespace JobTracker.Shared.DTOs;

/// <summary>
/// Request to create a new job application.
/// </summary>
public class CreateApplicationRequest
{
  public string Company { get; set; } = string.Empty;
  public string RoleTitle { get; set; } = string.Empty;
  public string? Location { get; set; }
  public string? SalaryRange { get; set; }
  public DateTime AppliedDate { get; set; }
  public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
  public ApplicationSource Source { get; set; }
  public string? Url { get; set; }
  public string? ResumeUsed { get; set; }
  public string? Notes { get; set; }
}
