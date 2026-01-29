using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetCategories.Command.DeleteAssetCategories
{
    public class DeleteAssetCategoriesCommand : IRequest<int>
    {
        public int Id { get; set; }
    }
}