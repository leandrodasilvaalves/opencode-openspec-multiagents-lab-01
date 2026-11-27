using System.Text.RegularExpressions;
using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Exceptions;

namespace Lab.Orders.Domain.Customers;

public record Address
{
    public string Street { get; init; }
    public string Number { get; init; }
    public string? Complement { get; init; }
    public string Neighborhood { get; init; }
    public string City { get; init; }
    public string State { get; init; }
    public string PostalCode { get; init; }

    public Address(string street, string number, string? complement, string neighborhood, string city, string state, string postalCode)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainRuleException(new[] { new Error("ADDRESS_REQUIRED_STREET", "Street is required.") });
        if (string.IsNullOrWhiteSpace(number))
            throw new DomainRuleException(new[] { new Error("ADDRESS_REQUIRED_NUMBER", "Number is required.") });
        if (string.IsNullOrWhiteSpace(neighborhood))
            throw new DomainRuleException(new[] { new Error("ADDRESS_REQUIRED_NEIGHBORHOOD", "Neighborhood is required.") });
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainRuleException(new[] { new Error("ADDRESS_REQUIRED_CITY", "City is required.") });
        if (string.IsNullOrWhiteSpace(state) || state.Length != 2)
            throw new DomainRuleException(new[] { new Error("ADDRESS_INVALID_STATE", "State must be a 2-letter UF.") });
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new DomainRuleException(new[] { new Error("ADDRESS_REQUIRED_POSTAL_CODE", "Postal code is required.") });

        var cepDigits = OnlyDigits(postalCode);
        if (cepDigits.Length != 8)
            throw new DomainRuleException(new[] { new Error("ADDRESS_INVALID_POSTAL_CODE", "Postal code must have 8 digits.") });

        Street = street.Trim();
        Number = number.Trim();
        Complement = string.IsNullOrWhiteSpace(complement) ? null : complement.Trim();
        Neighborhood = neighborhood.Trim();
        City = city.Trim();
        State = state.Trim().ToUpperInvariant();
        PostalCode = cepDigits;
    }

    private static string OnlyDigits(string input) => Regex.Replace(input, @"\D", string.Empty);
}
