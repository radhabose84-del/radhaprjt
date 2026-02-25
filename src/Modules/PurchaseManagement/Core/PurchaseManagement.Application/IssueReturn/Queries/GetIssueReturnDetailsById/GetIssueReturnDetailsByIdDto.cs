namespace PurchaseManagement.Application.IssueReturn.Queries.GetIssueReturnDetailsById
{
    public class GetIssueReturnDetailsByIdDto
    {
        public int Id { get; set; }
        public int RequestCategoryId { get; set; }
        public string? RequestCategoryName { get; set; }
        public int UnitId { get; set; }
        public string? IssueReturnNo { get; set; }
        public DateTimeOffset IssueReturnDate { get; set; }
        public int? IssueHeaderId { get; set; }
        public int DepartmentId { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTimeOffset? ApprovedDate { get; set; }
        public string? ApprovedByName { get; set; }
        public string? ApprovedIP { get; set; }
        public string? Remarks { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public List<GetIssueReturnDetailsDto>? getIssueReturnDetails { get; set; }
        public class GetIssueReturnDetailsDto
        {
        public int Id { get; set; }
        public int IssueReturnHeaderId { get; set; }
        public int ItemId { get; set; }
        public int UomId { get; set; }
        public int WarehouseStockId { get; set; }
        public int StorageTypeId { get; set; }
        public int TargetId { get; set; }
        public decimal ReturnQuantity { get; set; }
        public decimal ReturnValue { get; set; }
        public int ReasonId { get; set; }
        public string? Remarks { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTimeOffset? ApprovedDate { get; set; }
        public string? ApprovedByName { get; set; }
        public string? ApprovedIP { get; set; }
        public int StatusId { get; set; }
        public string? StatusName { get; set; }
        public int? SubStoresDepartmentId { get; set; }
        }
    }
}