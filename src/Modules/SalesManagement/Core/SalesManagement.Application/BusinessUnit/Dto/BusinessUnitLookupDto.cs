
namespace SalesManagement.Application.BusinessUnit.Dto
{
    public sealed class BusinessUnitLookupDto
    {
        public int Id { get; set; }
        public string BusinessUnitCode { get; set; } = null!;
        public string BusinessUnitName { get; set; } = null!;
    }
}
