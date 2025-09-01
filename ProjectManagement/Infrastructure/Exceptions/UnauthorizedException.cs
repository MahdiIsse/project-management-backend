namespace ProjectManagement.Infrastructure.Exceptions;

public class UnauthorizedException : Exception
{
  public UnauthorizedException(string message = "Access denied. Authentication required.") : base(message) { }
}
