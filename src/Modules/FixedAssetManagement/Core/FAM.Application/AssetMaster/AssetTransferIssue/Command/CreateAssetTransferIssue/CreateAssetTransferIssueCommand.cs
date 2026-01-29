using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAssetTransfered;
using FAM.Application.Common.HttpResponse;
using FAM.Domain.Entities.AssetMaster;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Command.CreateAssetTransferIssue
{
    public class CreateAssetTransferIssueCommand   : IRequest<int>
    {

      public AssetTransferIssueHdrDto? AssetTransferIssueHdrDto { get; set; }
    }
}