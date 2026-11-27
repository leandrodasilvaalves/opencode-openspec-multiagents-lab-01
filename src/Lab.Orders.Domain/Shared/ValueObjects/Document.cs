using System.Globalization;
using System.Text.RegularExpressions;
using Lab.Orders.Domain.Contracts;
using Lab.Orders.Domain.Exceptions;

namespace Lab.Orders.Domain.Shared.ValueObjects;

public class Document : ValueObject<Document>
{
    public string Number { get; }
    public string Type { get; }

    private Document(string number, string type)
    {
        Number = number;
        Type = type;
        ValidateThrow();
    }

    public static Document Cpf(string cpf)
    {
        var digits = OnlyDigits(cpf);
        if (digits.Length != 11)
        {
            throw new DomainRuleException(new[]
            {
                new Error("DOCUMENT_INVALID_CPF", "CPF must have 11 digits.")
            });
        }

        if (!ValidateCpfDv(digits))
        {
            throw new DomainRuleException(new[]
            {
                new Error("DOCUMENT_INVALID_CPF_DV", "CPF check digits are invalid.")
            });
        }

        return new Document(digits, "CPF");
    }

    public static Document Cnpj(string cnpj)
    {
        var digits = OnlyDigits(cnpj);
        if (digits.Length != 14)
        {
            throw new DomainRuleException(new[]
            {
                new Error("DOCUMENT_INVALID_CNPJ", "CNPJ must have 14 digits.")
            });
        }

        if (!ValidateCnpjDv(digits))
        {
            throw new DomainRuleException(new[]
            {
                new Error("DOCUMENT_INVALID_CNPJ_DV", "CNPJ check digits are invalid.")
            });
        }

        return new Document(digits, "CNPJ");
    }

    public static Document FromString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new DomainRuleException(new[]
            {
                new Error("DOCUMENT_REQUIRED", "Document is required.")
            });
        }

        var digits = OnlyDigits(input);
        if (digits.Length == 0)
        {
            throw new DomainRuleException(new[]
            {
                new Error("DOCUMENT_NUMERIC_ONLY", "Document must contain only numeric digits.")
            });
        }

        if (digits.Length == 11)
        {
            return Cpf(digits);
        }

        if (digits.Length == 14)
        {
            return Cnpj(digits);
        }

        throw new DomainRuleException(new[]
        {
            new Error("DOCUMENT_UNKNOWN_FORMAT", "Document format is not recognized. Expected 11 digits (CPF) or 14 digits (CNPJ).")
        });
    }

    private static string OnlyDigits(string input) => Regex.Replace(input, @"\D", string.Empty);

    private static bool ValidateCpfDv(string digits)
    {
        if (digits.Length != 11) return false;

        var sum = 0;
        for (var i = 0; i < 9; i++)
        {
            sum += (digits[i] - '0') * (10 - i);
        }

        var remainder = sum % 11;
        var firstDv = remainder < 2 ? 0 : 11 - remainder;
        if ((digits[9] - '0') != firstDv) return false;

        sum = 0;
        for (var i = 0; i < 10; i++)
        {
            sum += (digits[i] - '0') * (11 - i);
        }

        remainder = sum % 11;
        var secondDv = remainder < 2 ? 0 : 11 - remainder;
        return (digits[10] - '0') == secondDv;
    }

    private static bool ValidateCnpjDv(string digits)
    {
        if (digits.Length != 14) return false;

        var firstWeights = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            sum += (digits[i] - '0') * firstWeights[i];
        }

        var remainder = sum % 11;
        var firstDv = remainder < 2 ? 0 : 11 - remainder;
        if ((digits[12] - '0') != firstDv) return false;

        var secondWeights = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        sum = 0;
        for (var i = 0; i < 13; i++)
        {
            sum += (digits[i] - '0') * secondWeights[i];
        }

        remainder = sum % 11;
        var secondDv = remainder < 2 ? 0 : 11 - remainder;
        return (digits[13] - '0') == secondDv;
    }

    protected override string ErrorCode() => "DOCUMENT";

    protected override void Validate()
    {
    }

    public override bool Equals(Document? other) => other is not null && Number.Equals(other.Number) && Type.Equals(other.Type);

    public override bool Equals(object? obj) => Equals(obj as Document);

    public override int GetHashCode() => (Number.GetHashCode() * 907) + Type.GetHashCode();

    public override string ToString() => Number;
}
