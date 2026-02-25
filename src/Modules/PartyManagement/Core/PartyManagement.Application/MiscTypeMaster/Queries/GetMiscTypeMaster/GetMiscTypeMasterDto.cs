using static PartyManagement.Domain.Common.BaseEntity;

namespace PartyManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMaster
{
    public class GetMiscTypeMasterDto
    {
        public int Id { get; set; }
        public string? MiscTypeCode { get; set; }
        public string? Description { get; set; } 
        public Status  IsActive { get; set; }
        public IsDelete IsDeleted { get; set; }
    }
}