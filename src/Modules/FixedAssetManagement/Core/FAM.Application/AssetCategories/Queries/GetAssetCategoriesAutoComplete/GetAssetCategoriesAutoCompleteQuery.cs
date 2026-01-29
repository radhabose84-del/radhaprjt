using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetCategories.Queries.GetAssetCategoriesAutoComplete
{
    public class GetAssetCategoriesAutoCompleteQuery : IRequest<List<AssetCategoriesAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}