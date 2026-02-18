using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetCategoryByCustodian
{
    public class GetCategoryByCustodianQuery : IRequest<List<GetCategoryByCustodianDto>>
    {
          public int DepartmentId { get; set; }
        public string? CustodianId { get; set; }      
        
    }
}