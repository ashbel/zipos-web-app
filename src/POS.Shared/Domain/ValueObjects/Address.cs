namespace POS.Shared.Domain.ValueObjects;

public record Address(
    string Street,
    string City,
    string State,
    string PostalCode,
    string Country
);