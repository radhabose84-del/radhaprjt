namespace SalesManagement.Application.SalesOrder.Dto
{
    public sealed class SalesOrderLookupDto
    {
        public int Id { get; set; }
        public string? SalesOrderNo { get; set; }
        public DateOnly OrderDate { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
        public int? AmendmentStatusId { get; set; }
        public string? AmendmentStatusName { get; set; }
    }
}
