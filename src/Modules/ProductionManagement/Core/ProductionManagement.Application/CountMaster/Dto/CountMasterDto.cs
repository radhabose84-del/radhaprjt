namespace ProductionManagement.Application.CountMaster.Dto
{
    public class CountMasterDto
    {
        public int Id { get; set; }
        public string? CountCode { get; set; }
        public decimal CountValue { get; set; }
        public string? ShortName { get; set; }
        public int? CountCategoryId { get; set; }
        public string? CountCategoryName { get; set; }
        public int CountTypeId { get; set; }
        public string? CountTypeName { get; set; }
        public string? CountDescription { get; set; }
        public int UOMId { get; set; }
        public string? UOMCode { get; set; }
        public string? UOMName { get; set; }
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
