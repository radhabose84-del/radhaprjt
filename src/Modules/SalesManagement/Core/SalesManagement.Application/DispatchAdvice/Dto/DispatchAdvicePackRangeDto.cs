namespace SalesManagement.Application.DispatchAdvice.Dto
{
    public class DispatchAdvicePackRangeDto
    {
        public int SNo { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int LotId { get; set; }
        public string? LotName { get; set; }
        public int PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
        public int FromPackNo { get; set; }
        public int ToPackNo { get; set; }
        public int TotalPacks { get; set; }
    }
}
