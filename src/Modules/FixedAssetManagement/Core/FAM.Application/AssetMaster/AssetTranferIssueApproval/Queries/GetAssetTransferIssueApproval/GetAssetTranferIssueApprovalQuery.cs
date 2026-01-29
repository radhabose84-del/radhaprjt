using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueApproval
{
    public class GetAssetTranferIssueApprovalQuery :  IRequest<ApiResponseDTO<List<AssetTransferIssueApprovalDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; } 
        public string? SearchTerm { get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
    }
}