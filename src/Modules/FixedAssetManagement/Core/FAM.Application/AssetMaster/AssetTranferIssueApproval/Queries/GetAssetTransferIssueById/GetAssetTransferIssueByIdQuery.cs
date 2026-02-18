using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueApproval;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTranferIssueApproval.Queries.GetAssetTransferIssueById
{
    public class GetAssetTransferIssueByIdQuery : IRequest<List<AssetTransferIssueByIdDto>>
    {
        public int Id {get; set;}
    }
}