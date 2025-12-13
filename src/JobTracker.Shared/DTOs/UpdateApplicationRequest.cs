namespace JobTracker.Shared.DTOs;

/// <summary>
/// Request to update an existing job application.
/// </summary>
public class UpdateApplicationRequest
{
  public string? Company { get; set; }
  public string? RoleTitle { get; set; }
  public string? Location { get; set; }
  public string? SalaryRange { get; set; }
  public string? Url { get; set; }
  public string? ResumeUsed { get; set; }
  public string? Notes { get; set; }
}
