using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetSubCategories.Command.UpdateAssetSubCategories
{
    public class UpdateAssetSubCategoriesCommand:IRequest<int>
    {
        public int Id { get; set; }
        public string? SubCategoryName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public int AssetCategoriesId { get; set; }
        public byte IsActive { get; set; }
    }
}