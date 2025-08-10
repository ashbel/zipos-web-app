namespace POS.Shared.Domain.Exceptions;

public class BusinessRuleException : DomainException
{
    public BusinessRuleException(string rule, string message, params object[] parameters)
        : base($"BUSINESS_RULE_{rule}", message, parameters)
    {
    }
}