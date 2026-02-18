using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetSubCategories.Queries.GetAssetSubCategoriesByCategoryId
{
    public class GetAssetSubCategoriesByCategoryIdQuery : IRequest<List<AssetSubCategoriesAutoCompleteDto>>
    {
        public int AssetCategoriesId { get; set; }
    }
        
    
}