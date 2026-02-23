#nullable disable

namespace SalesManagement.Application.SalesOffice.Dto
{
    public sealed class SalesOfficeLookupDto
    {
        public int Id { get; set; }
        public string SalesOfficeName { get; set; }
        public int SalesOrganisationId { get; set; }
    }
}
