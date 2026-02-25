namespace PurchaseManagement.Application.PurchaseOrder.ServicePO.Queries.ServiceEntrySheet.GetPoServiceHeaderByPoId
{
    public class PoServiceHeaderByIdDto
    {
        public string PONumber { get; set; } = "";
        public DateTimeOffset PODate { get; set; }
        public int VendorId { get; set; }

        public string? VendorCode { get; set; }
        public string? VendorName { get; set; }
        public int ServiceCategoryId { get; set; }
        public string? ServiceCategory { get; set; }
        public int ContractTypeId { get; set; }
        public string? ContractType { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public DateTimeOffset? ValidityFrom { get; set; }
        public DateTimeOffset? ValidityTo { get; set; }
    }
}