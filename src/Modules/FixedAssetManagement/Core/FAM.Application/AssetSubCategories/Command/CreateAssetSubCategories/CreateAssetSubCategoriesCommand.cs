using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetSubCategories.Command.CreateAssetSubCategories
{
    public class CreateAssetSubCategoriesCommand :IRequest<int>
    {
        //public string? Code { get; set; }
        public string? SubCategoryName { get; set; }
        public string? Description { get; set; }
        public int AssetCategoriesId { get; set; }
    }
}