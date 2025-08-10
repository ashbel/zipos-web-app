namespace POS.Shared.Domain.Exceptions;

public abstract class DomainException : Exception
{
    public string ErrorCode { get; }
    public object[] Parameters { get; }
    
    protected DomainException(string errorCode, string message, params object[] parameters) 
        : base(message)
    {
        ErrorCode = errorCode;
        Parameters = parameters;
    }
}