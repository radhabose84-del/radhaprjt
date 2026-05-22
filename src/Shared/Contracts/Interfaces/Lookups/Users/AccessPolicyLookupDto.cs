namespace Contracts.Interfaces.Lookups.Users;

public sealed class AccessPolicyLookupDto
{
    public int    Id         { get; set; }
    public string PolicyCode { get; set; } = string.Empty;
    public string PolicyName { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string FieldName  { get; set; } = string.Empty;
}
