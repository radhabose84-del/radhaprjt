using MediatR;

namespace FAM.Application.AssetMaster.AssetAdditionalCost.Commands.UpdateAssetAdditionalCost
{
    public class UpdateAssetAdditionalCostCommand  :IRequest<int>
    { 
        public int Id {get; set;}
        public decimal Amount { get; set; }
        public string? JournalNo { get; set; }
        public int? CostType { get; set; } 
    }
}