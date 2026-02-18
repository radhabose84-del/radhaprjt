using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetTransferReceipt.Queries.GetAssetReceiptDetailsById
{
    public class GetAssetReceiptDetailsByIdQuery : IRequest<List<AssetReceiptDetailsByIdDto>>
    {
        public int AssetReceiptId { get; set; }
    }
}