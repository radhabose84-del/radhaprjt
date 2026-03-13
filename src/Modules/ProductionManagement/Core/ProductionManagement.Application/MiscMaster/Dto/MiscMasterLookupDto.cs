namespace ProductionManagement.Application.MiscMaster.Dto
{
    public sealed class MiscMasterLookupDto
    {
        public int Id { get; set; }
        public int MiscTypeId { get; set; }
        public string? MiscTypeCode { get; set; }
        public string? Code { get; set; }
        public string? Description { get; set; }
    }
}
