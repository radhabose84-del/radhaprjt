namespace PurchaseManagement.Application.MixCodeMaster.Dto
{
    public class MixCodeMasterDto
    {
        public int Id { get; set; }
        public string MixCode { get; set; } = default!;
        public string MixCodeDesc { get; set; } = default!;
        public int IsActive { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
    }
}
