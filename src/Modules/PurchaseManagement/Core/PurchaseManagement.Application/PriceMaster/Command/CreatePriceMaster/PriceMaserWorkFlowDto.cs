
namespace PurchaseManagement.Application.PriceMaster.Command.CreatePriceMaster
{
    public class PriceMaserWorkFlowDto
    {
        public int Id { get; set; }
        public string? ItemId { get; set; }
        public int VendorId { get; set; }
        public int StatusId { get; set; }
        public int UnitId { get; set; }               
    }
}