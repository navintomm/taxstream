namespace TaxStream.Core.Exceptions;

public class AuthorizationException : DomainException
{
    public AuthorizationException(string message = "You are not authorized to access this resource.")
        : base(message) { }
}
