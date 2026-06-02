namespace FinanceManagement.Application.TransactionTypeMaster.Dto
{
    public sealed class TransactionTypeMasterLookupDto
    {
        public int Id { get; set; }
        public string? TypeName { get; set; }
        public string? ShortName { get; set; }
        public bool IsGate { get; set; }
    }
}
