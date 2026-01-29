using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferIssue.Queries.GetAllAssetTransfer
{
    public class GetAllTransferQuery : IRequest<List<GetAllTransferDtlDto>>
    
    {
       public int AssetTransferId  { get; set; }        
    }
}