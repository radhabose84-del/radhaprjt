namespace UserManagement.Application.Companies.Queries.GetCompanies
{
    public class CompanyDTO
    {
        public string? CompanyName { get; set; }
        public string? LegalName { get; set; }
        public string? GstNumber { get; set; }
        public string? TIN { get; set; }
        public string? TAN { get; set; }
        public string? CSTNo { get; set; }
        public int YearOfEstablishment { get; set; }
        public string? Website { get; set; }
        public int EntityId { get; set; }
        public string? Logo { get; set; }
        public string? PanNumber { get; set; }
        // public IFormFile File { get; set; }
        public CompanyAddressDTO? CompanyAddress { get; set; }
        public CompanyContactDTO? CompanyContact { get; set; }
    }
}