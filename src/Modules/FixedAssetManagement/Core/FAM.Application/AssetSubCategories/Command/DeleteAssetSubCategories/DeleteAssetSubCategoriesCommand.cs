using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetSubCategories.Command.DeleteAssetSubCategories
{
    public class DeleteAssetSubCategoriesCommand :IRequest<int>
    {
         public int Id { get; set; }
    }
}