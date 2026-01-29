using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetIssueDetailsById
{
    public class GetAssetIssueDetailsByIdQuery  : IRequest<ApiResponseDTO<AssetTransferJsonDto>>
    {
         public int AssetTransferId { get; set; }
    }
}