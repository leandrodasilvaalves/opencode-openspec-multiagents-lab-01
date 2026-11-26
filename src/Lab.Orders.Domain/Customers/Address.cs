using Lab.Orders.Domain.Shared;

namespace Lab.Orders.Domain.Customers;

public sealed record Address
{
    public const int CepLength = 8;

    private static readonly HashSet<string> ValidStates = new(StringComparer.OrdinalIgnoreCase)
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG",
        "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    public string Street { get; }
    public string Number { get; }
    public string? Complement { get; }
    public string Neighborhood { get; }
    public string City { get; }
    public string State { get; }
    public string PostalCode { get; }

    private Address(string street, string number, string? complement, string neighborhood, string city, string state, string postalCode)
    {
        Street = street;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        PostalCode = postalCode;
    }

    public static Address Create(
        string? street,
        string? number,
        string? complement,
        string? neighborhood,
        string? city,
        string? state,
        string? postalCode)
    {
        var normalizedStreet = RequireNonEmpty(street, nameof(street));
        var normalizedNumber = RequireNonEmpty(number, nameof(number));
        var normalizedNeighborhood = RequireNonEmpty(neighborhood, nameof(neighborhood));
        var normalizedCity = RequireNonEmpty(city, nameof(city));
        var normalizedState = NormalizeState(state);
        var normalizedPostalCode = NormalizePostalCode(postalCode);
        var normalizedComplement = string.IsNullOrWhiteSpace(complement) ? null : complement.Trim();

        return new Address(
            normalizedStreet,
            normalizedNumber,
            normalizedComplement,
            normalizedNeighborhood,
            normalizedCity,
            normalizedState,
            normalizedPostalCode);
    }

    private static string RequireNonEmpty(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException($"Address field '{fieldName}' is required and cannot be empty.");

        return value.Trim();
    }

    private static string NormalizeState(string? state)
    {
        if (string.IsNullOrWhiteSpace(state))
            throw new DomainException("Address field 'state' is required and cannot be empty.");

        var uf = state.Trim().ToUpperInvariant();

        if (uf.Length != 2 || !ValidStates.Contains(uf))
            throw new DomainException("Address state must be a valid Brazilian UF.");

        return uf;
    }

    private static string NormalizePostalCode(string? postalCode)
    {
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new DomainException("Address field 'postalCode' is required and cannot be empty.");

        var trimmed = postalCode.Trim();

        foreach (var c in trimmed)
        {
            if (!char.IsDigit(c) && c != '-' && c != ' ')
                throw new DomainException("PostalCode (CEP) must contain numeric digits only.");
        }

        var digits = new string(trimmed.Where(char.IsDigit).ToArray());

        if (digits.Length != CepLength)
            throw new DomainException("PostalCode (CEP) must have 8 digits.");

        return digits;
    }
}
