using JobTracker.Shared.Models;

namespace JobTracker.Shared.DTOs;

/// <summary>
/// Request to create a new interview event.
/// </summary>
public class CreateInterviewRequest
{
  public DateTime Date { get; set; }
  public string? Interviewer { get; set; }
  public InterviewType Type { get; set; }
  public string? Notes { get; set; }
}
