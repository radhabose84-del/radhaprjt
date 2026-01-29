using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FAM.Domain.Common.BaseEntity;

namespace FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories
{
    public class AssetSubCategoriesDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? SubCategoryName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public int AssetCategoriesId { get; set; }
        public string? AssetCategoriesName { get; set; }
        public Status IsActive { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public string? CreatedByName { get; set; }

    }
}