namespace MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroupById
{
    public class GetFeederGroupByIdDto
    {
         public int Id { get; set; }
        public string? FeederGroupCode { get; set; }
        public string? FeederGroupName { get; set; }
         public int  UnitId { get; set; }
        public bool IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? ModifiedByName { get; set; }
        public string? CreatedIP { get; set; }
        public string? ModifiedIP { get; set; }
    }
}