using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptPending;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Command.CreateAssetTransferReceipt
{
    public class CreateAssetTransferReceiptCommand   : IRequest<int>
    {
          public AssetTransferReceiptHdrDto? AssetTransferReceiptHdrDto { get; set; }
    }
}