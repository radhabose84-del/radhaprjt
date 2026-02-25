using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.AssetGroup.Queries.GetAssetGroup
{
    public class AssetGroupDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? GroupName { get; set; }
        public int SortOrder { get; set; }
        public Status IsActive { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public decimal? GroupPercentage { get; set; }


    }
}