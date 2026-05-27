namespace QCManagement.Application.MiscMaster.Dto
{
    public class MiscMasterDto
    {
        public int Id { get; set; }
        public int MiscTypeId { get; set; }
        public string? MiscTypeCode { get; set; }
        public string? MiscTypeDescription { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
