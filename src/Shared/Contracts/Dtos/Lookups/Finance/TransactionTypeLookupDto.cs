namespace Contracts.Dtos.Lookups.Finance
{
    public sealed class TransactionTypeLookupDto
    {
        public int Id { get; set; }
        public string? TypeName { get; set; }
        public string? ShortName { get; set; }
        public string? Description { get; set; }
    }
}
