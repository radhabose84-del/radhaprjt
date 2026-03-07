namespace SalesManagement.Application.DeliveryChallan.Dto
{
    public sealed class DeliveryChallanLookupDto
    {
        public int Id { get; set; }
        public string? DeliveryNumber { get; set; }
        public DateOnly DeliveryDate { get; set; }
        public string? StoNumber { get; set; }
    }
}
