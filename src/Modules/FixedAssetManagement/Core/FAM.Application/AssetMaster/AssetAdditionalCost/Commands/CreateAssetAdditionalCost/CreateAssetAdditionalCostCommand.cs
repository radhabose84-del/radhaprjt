using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetAdditionalCost.Commands.CreateAssetAdditionalCost
{
    public class CreateAssetAdditionalCostCommand :IRequest<int>
    {
        public int AssetId { get; set; }   
        public int AssetSourceId { get; set; }   
        public decimal Amount { get; set; }
        public string? JournalNo { get; set; }
        public int? CostType { get; set; } 
    }
}