using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.AssetCategories.Queries.GetAssetCategories
{
    public class AssetCategoriesDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public int AssetGroupId { get; set; }
        public string? AssetGroupName { get; set; }
        public Status IsActive { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedByName { get; set; }




    }
}