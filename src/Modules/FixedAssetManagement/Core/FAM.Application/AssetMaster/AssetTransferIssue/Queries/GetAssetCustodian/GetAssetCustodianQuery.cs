using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetCustodian
{
    public class GetAssetCustodianQuery : IRequest<List<GetAssetCustodianDto>>
    {
        public string? OldUnitId { get; set; }
        
        public int DepartmentId { get; set; }
    }
}