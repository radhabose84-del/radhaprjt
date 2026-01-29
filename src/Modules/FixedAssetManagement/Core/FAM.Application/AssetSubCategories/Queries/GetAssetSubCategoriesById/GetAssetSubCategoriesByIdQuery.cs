using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetSubCategories.Queries.GetAssetSubCategoriesById
{
    public class GetAssetSubCategoriesByIdQuery: IRequest<AssetSubCategoriesDto>
    {
        public int Id { get; set; }
    }
}