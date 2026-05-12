namespace SalesManagement.Application.SalesAgreement.Dto
{
    public class CreateSalesAgreementDetailDto
    {
        public int ItemId { get; set; }
        public int? VariantId { get; set; }
        public decimal AgreedRate { get; set; }
        public decimal TotalQty { get; set; }
    }
}
