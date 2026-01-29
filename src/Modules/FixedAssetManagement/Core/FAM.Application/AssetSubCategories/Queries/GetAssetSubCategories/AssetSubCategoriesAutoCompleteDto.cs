using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories
{
    public class AssetSubCategoriesAutoCompleteDto
    {
        public int Id { get; set; }
        public string? SubCategoryName { get; set; }
    }
}