using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssertByCategory
{
    public class GetAssetsByCategoryQuery  : IRequest<List<GetAssetMasterDto>>
    {
         public int AssetCategoryId { get; set; }
         public int AssetDepartmentId { get; set;}
    }
}