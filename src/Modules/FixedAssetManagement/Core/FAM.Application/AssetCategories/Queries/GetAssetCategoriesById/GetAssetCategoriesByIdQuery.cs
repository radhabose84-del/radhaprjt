using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetCategories.Queries.GetAssetCategoriesById
{
    public class GetAssetCategoriesByIdQuery : IRequest<AssetCategoriesDto>
    {
        public int Id { get; set; }
    }
}