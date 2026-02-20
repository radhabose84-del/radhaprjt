#nullable disable
namespace SalesManagement.Application.SalesOrganisation.Dto
{
    public sealed class SalesOrganisationLookupDto
    {
        public int Id { get; set; }
        public string SalesOrganisationCode { get; set; } = default!;
        public string SalesOrganisationName { get; set; } = default!;
    }
}
