using MediatR;

namespace FAM.Application.AssetMaster.AssetDisposal.Command.UpdateAssetDisposal
{
    public class UpdateAssetDisposalCommand :IRequest<int>
    {
        public int Id { get; set; } 
        public DateOnly DisposalDate { get; set; }
        public int? DisposalType { get; set; }  
        public string? DisposalReason { get; set; }
        public decimal? DisposalAmount { get; set; }
    }
}