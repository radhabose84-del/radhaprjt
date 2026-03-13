namespace Contracts.Dtos.Lookups.Party;

public sealed class CustomerLookupDto
{
    public int Id { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
}
