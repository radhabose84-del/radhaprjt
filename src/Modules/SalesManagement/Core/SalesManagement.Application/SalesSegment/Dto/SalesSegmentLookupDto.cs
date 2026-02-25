
namespace SalesManagement.Application.SalesSegment.Dto
{
    public sealed class SalesSegmentLookupDto
    {
        public int Id { get; set; }
        public string SegmentName { get; set; } = null!;
        public int SalesOrganisationId { get; set; }
        public int SalesChannelId { get; set; }
        public int BusinessUnitId { get; set; }
    }
}
