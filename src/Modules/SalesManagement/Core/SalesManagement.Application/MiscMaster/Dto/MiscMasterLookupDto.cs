namespace SalesManagement.Application.MiscMaster.Dto
{
    public class MiscMasterLookupDto
    {
        public int Id { get; set; }
        public int MiscTypeId { get; set; }
        public string MiscTypeCode { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}
