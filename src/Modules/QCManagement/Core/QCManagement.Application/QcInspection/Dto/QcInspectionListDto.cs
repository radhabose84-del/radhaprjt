namespace QCManagement.Application.QcInspection.Dto
{
    public class QcInspectionListDto
    {
        public int Id { get; set; }
        public string? QcInspectionNo { get; set; }
        public int GrnHeaderId { get; set; }
        public int GrnDetailId { get; set; }
        public string? GrnNo { get; set; }
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public string? BatchNumber { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal? AcceptedQuantity { get; set; }
        public decimal? RejectedQuantity { get; set; }
        public int? QcStatusId { get; set; }
        public string? QcStatusCode { get; set; }
        public string? QcStatusName { get; set; }
        public DateTimeOffset InspectionDate { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
    }
}
