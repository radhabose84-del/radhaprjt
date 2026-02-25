using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.MiscMaster.Queries.GetMiscMaster
{
    public class GetMiscMasterDto
    {
        public int Id { get; set; }
        public int MiscTypeId { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public Status IsActive { get; set; }
        public IsDelete IsDeleted { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        
    }
}