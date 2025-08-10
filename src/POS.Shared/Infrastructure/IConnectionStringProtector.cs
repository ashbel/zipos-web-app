namespace POS.Shared.Infrastructure;

public interface IConnectionStringProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedValue);
}

