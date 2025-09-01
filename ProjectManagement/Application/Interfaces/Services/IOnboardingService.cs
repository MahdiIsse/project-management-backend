namespace ProjectManagement.Application.Interfaces.Services;

public interface IOnboardingService
{
  Task<(bool Success, string? Error)> CreateInitialDataAsync(string userId);
}
