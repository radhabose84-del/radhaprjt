using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetCategories.Queries.GetAssetCategoriesByAssetGroupId
{
    public class GetAssetCategoriesByAssetGroupIdQuery : IRequest<List<AssetCategoriesAutoCompleteDto>>
    {
        public int AssetGroupId { get; set; }
    }
}