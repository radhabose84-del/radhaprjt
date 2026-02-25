using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.SubLocation.Queries.GetSubLocations
{
    public class SubLocationDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? SubLocationName { get; set; }
        public string? Description { get; set; }
        public int UnitId { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public int LocationId { get; set; }
        public Status IsActive { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
    }
}