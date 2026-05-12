namespace SalesManagement.Application.SalesAgreement.Dto
{
    public class UpdateSalesAgreementDetailDto
    {
        // Null/0 = new line (insert); > 0 = existing line (update).
        public int? Id { get; set; }

        public int ItemId { get; set; }
        public int? VariantId { get; set; }
        public decimal AgreedRate { get; set; }
        public decimal TotalQty { get; set; }
    }
}
