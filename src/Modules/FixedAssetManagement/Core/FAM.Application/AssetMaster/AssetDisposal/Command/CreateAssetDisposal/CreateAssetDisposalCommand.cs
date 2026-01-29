using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetDisposal.Command.CreateAssetDisposal
{
    public class CreateAssetDisposalCommand :IRequest<int>
    {
        public int AssetId { get; set; } 
        public int AssetPurchaseId { get; set; } 
        public DateOnly DisposalDate { get; set; }
        public int? DisposalType { get; set; }  
        public string? DisposalReason { get; set; }
        public decimal? DisposalAmount { get; set; }
    }
}