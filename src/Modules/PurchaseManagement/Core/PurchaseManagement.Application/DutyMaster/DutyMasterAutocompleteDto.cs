namespace PurchaseManagement.Application.DutyMaster
{
    public sealed class DutyMasterAutocompleteDto
    {
        public int Id { get; set; }
        public string DutyCode { get; set; } = default!;
        public string? Description { get; set; }
        public string TariffNumber { get; set; } = default!;
        public string? HsnCode { get; set; }
        public int HsnId { get; set; }
    }
}
