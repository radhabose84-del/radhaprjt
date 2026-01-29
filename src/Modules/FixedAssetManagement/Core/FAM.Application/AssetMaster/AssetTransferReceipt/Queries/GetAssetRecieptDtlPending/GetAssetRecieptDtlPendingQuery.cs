using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetailsById;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetRecieptDtlPending
{
    public class GetAssetRecieptDtlPendingQuery   : IRequest<AssetTrasnferReceiptHdrPendingDto>
    {
         public int AssetTransferId { get; set; }
    }
}