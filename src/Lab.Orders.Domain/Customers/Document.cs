using Lab.Orders.Domain.Shared;

namespace Lab.Orders.Domain.Customers;

public readonly record struct Document
{
    public const int CpfLength = 11;
    public const int CnpjLength = 14;

    public string Value { get; }
    public DocumentKind Kind { get; }

    private Document(string value, DocumentKind kind)
    {
        Value = value;
        Kind = kind;
    }

    public static Document Create(string? value)
    {
        var digits = Normalize(value);

        if (digits.Length == CpfLength)
        {
            if (!IsValidCpf(digits))
                throw new DomainException("Invalid CPF check digits.");

            return new Document(digits, DocumentKind.Cpf);
        }

        if (digits.Length == CnpjLength)
        {
            if (!IsValidCnpj(digits))
                throw new DomainException("Invalid CNPJ check digits.");

            return new Document(digits, DocumentKind.Cnpj);
        }

        throw new DomainException("Document must be CPF (11 digits) or CNPJ (14 digits), numeric only.");
    }

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Document is required.");

        var trimmed = value.Trim();

        foreach (var c in trimmed)
        {
            if (char.IsDigit(c) || c == '.' || c == '-' || c == '/' || c == ' ')
                continue;

            throw new DomainException("Document must contain numeric digits only; alphanumeric is rejected.");
        }

        return new string(trimmed.Where(char.IsDigit).ToArray());
    }

    private static bool IsValidCpf(string digits)
    {
        if (digits.Distinct().Count() == 1)
            return false;

        var first = CpfCheckDigit(digits.AsSpan(0, 9), 10);
        if (digits[9] - '0' != first)
            return false;

        Span<char> withFirst = stackalloc char[10];
        digits.AsSpan(0, 9).CopyTo(withFirst);
        withFirst[9] = (char)('0' + first);

        var second = CpfCheckDigit(withFirst, 11);
        return digits[10] - '0' == second;
    }

    private static int CpfCheckDigit(ReadOnlySpan<char> digits, int startWeight)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
            sum += (digits[i] - '0') * (startWeight - i);

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    private static bool IsValidCnpj(string digits)
    {
        if (digits.Distinct().Count() == 1)
            return false;

        int[] weights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] weights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var first = CnpjCheckDigit(digits.AsSpan(0, 12), weights1);
        if (digits[12] - '0' != first)
            return false;

        Span<char> withFirst = stackalloc char[13];
        digits.AsSpan(0, 12).CopyTo(withFirst);
        withFirst[12] = (char)('0' + first);

        var second = CnpjCheckDigit(withFirst, weights2);
        return digits[13] - '0' == second;
    }

    private static int CnpjCheckDigit(ReadOnlySpan<char> digits, int[] weights)
    {
        var sum = 0;
        for (var i = 0; i < digits.Length; i++)
            sum += (digits[i] - '0') * weights[i];

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}

public enum DocumentKind
{
    Cpf,
    Cnpj
}
