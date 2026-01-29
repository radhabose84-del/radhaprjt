using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetDtlToTransfer;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetBulkAssetToTransfer
{
    public class GetBulkAssetToTransferQuery : IRequest<ApiResponseDTO<List<GetAssetDetailsToTransferHdrDto>>>
    {

        public int DepartmentId { get; set; }
        public string? CustodianId { get; set; }        
        public string?  CategoryID { get; set; }
        
    }
}