// PurchaseManagement.Domain/ValueObjects/EmailAddress.cs
namespace PurchaseManagement.Domain.Entities.ValueObjects;
public sealed record EmailAddress
{
    public string Value { get; }
    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
            throw new ArgumentException("Invalid email address");
        Value = value.Trim();
    }
    public static implicit operator string(EmailAddress e) => e.Value;
}
