using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetCategoryByDeptId
{
    public class GetCategoryByDeptIQuery  :  IRequest<List<GetCategoryByDeptIdDto>>
    {    

        public int DepartmentId { get; set; }

        
    }
}