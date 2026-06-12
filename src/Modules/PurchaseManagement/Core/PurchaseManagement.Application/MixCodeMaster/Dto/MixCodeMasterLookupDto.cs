namespace PurchaseManagement.Application.MixCodeMaster.Dto
{
    public sealed class MixCodeMasterLookupDto
    {
        public int Id { get; set; }
        public string MixCode { get; set; } = default!;
        public string MixCodeDesc { get; set; } = default!;
    }
}
