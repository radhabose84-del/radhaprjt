namespace Contracts.Dtos.Purchase
{
    public class UpdateLineStatusDto
    {
        public int ModuleLineId { get; set; }
        // public decimal ApprovedQuantity { get; set; }
        public string Status { get; set; } = default!;
    }
}