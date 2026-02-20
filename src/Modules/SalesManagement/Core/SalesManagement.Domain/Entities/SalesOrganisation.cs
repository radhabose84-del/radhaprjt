#nullable disable
using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesOrganisation : BaseEntity
    {
        public string SalesOrganisationCode { get; set; }
        public string SalesOrganisationName { get; set; }
        public int CompanyId { get; set; }
    }
}
