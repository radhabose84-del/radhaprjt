#nullable disable

namespace SalesManagement.Application.SalesGroup.Dto
{
    public sealed class SalesGroupLookupDto
    {
        public int Id { get; set; }
        public string SalesGroupName { get; set; }
        public int SalesOfficeId { get; set; }
    }
}
