using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.Application.Location.Queries.GetLocationById
{
    public class LocationByIdDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? LocationName { get; set; }
        public string? Description { get; set; }
        public Status IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
        public IsDelete IsDeleted { get; set; }
    }
}
