using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FAM.Application.AssetCategories.Queries.GetAssetCategories
{
    public class AssetCategoriesAutoCompleteDto
    {
        public int Id { get; set; }
        public string? CategoryName { get; set; }
    }
}