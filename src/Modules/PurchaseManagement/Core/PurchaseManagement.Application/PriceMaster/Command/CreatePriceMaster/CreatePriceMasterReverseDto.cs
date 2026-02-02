

namespace PurchaseManagement.Application.PriceMaster.Command.CreatePriceMaster
{
    public class CreatePriceMasterReverseDto
    {
        public PriceMaserWorkFlowDto? Header { get; set; }
        public ICollection<PriceMaserWorkFlowDto>? Lines { get; set; }
    }
    
}