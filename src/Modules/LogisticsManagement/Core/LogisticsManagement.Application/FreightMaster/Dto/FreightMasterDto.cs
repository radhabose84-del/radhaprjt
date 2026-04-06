namespace LogisticsManagement.Application.FreightMaster.Dto
{
    public class FreightMasterDto
    {
        public int Id { get; set; }
        public int FreightModeId { get; set; }
        public string? FreightModeName { get; set; }
        public int RateMethodId { get; set; }
        public string? RateMethodName { get; set; }
        public decimal Rate { get; set; }
        public int ModuleId { get; set; }
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
