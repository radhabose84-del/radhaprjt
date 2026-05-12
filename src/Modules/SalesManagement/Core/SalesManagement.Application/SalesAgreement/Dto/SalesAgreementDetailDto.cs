namespace SalesManagement.Application.SalesAgreement.Dto
{
    public class SalesAgreementDetailDto
    {
        public int Id { get; set; }
        public int SalesAgreementHeaderId { get; set; }

        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }

        public int? VariantId { get; set; }
        public string? VariantName { get; set; }

        public int? UomId { get; set; }
        public string? UomName { get; set; }

        public decimal AgreedRate { get; set; }
        public decimal TotalQty { get; set; }
        public decimal ReleasedQty { get; set; }
        public decimal BalanceQty => TotalQty - ReleasedQty;
    }
}
