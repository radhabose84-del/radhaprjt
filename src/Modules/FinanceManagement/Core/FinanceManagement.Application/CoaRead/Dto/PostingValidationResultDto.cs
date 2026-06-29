namespace FinanceManagement.Application.CoaRead.Dto
{
    // US-GL02-16 (AC2) — validate-for-posting outcome. IsValid is false when the account is missing /
    // inactive, the currency mismatches, or a mandatory cost centre is absent; Reasons lists every cause.
    public class PostingValidationResultDto
    {
        public string? AccountCode { get; set; }
        public int? AccountId { get; set; }
        public bool IsValid { get; set; }
        public List<string> Reasons { get; set; } = new();
    }
}
