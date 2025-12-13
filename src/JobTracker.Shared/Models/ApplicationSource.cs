namespace JobTracker.Shared.Models;

/// <summary>
/// Represents the source where the job application was submitted.
/// </summary>
public enum ApplicationSource
{
  LinkedIn,
  Indeed,
  Glassdoor,
  CompanyWebsite,
  Referral,
  Recruiter,
  Other
}
