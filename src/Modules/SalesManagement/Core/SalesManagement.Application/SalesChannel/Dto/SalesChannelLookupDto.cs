namespace SalesManagement.Application.SalesChannel.Dto
{
    public sealed class SalesChannelLookupDto
    {
        public int Id { get; set; }
        public string SalesChannelCode { get; set; } = default!;
        public string SalesChannelName { get; set; } = default!;
    }
}
