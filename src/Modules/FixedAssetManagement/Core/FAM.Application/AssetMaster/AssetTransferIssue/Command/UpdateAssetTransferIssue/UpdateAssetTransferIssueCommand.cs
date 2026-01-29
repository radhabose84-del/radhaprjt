using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Command.UpdateAssetTransferIssue
{
    public class UpdateAssetTransferIssueCommand  : IRequest<bool>
    {
       
       public UpdateAssetTransferHdrDto? AssetTransferHdr  { get; set; }
    }
}