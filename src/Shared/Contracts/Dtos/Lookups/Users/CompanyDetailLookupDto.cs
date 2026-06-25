namespace Contracts.Dtos.Lookups.Users
{
    public sealed class CompanyDetailLookupDto
    {
        public int CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? LegalName { get; set; }
        public string? GstNumber { get; set; }
        public string? PanNumber { get; set; }
        public string? Website { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public int CityId { get; set; }
        public int StateId { get; set; }
        public string? PinCode { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        /// <summary>Stored logo path/file name (raw). Used server-side to read the file for base64.</summary>
        public string? Logo { get; set; }

        /// <summary>
        /// Public URL of the company logo, built from the 'CompanyLogoPath' base
        /// (AppData.MiscTypeMaster) + the stored logo file name. Null when no logo is set.
        /// </summary>
        public string? LogoUrl { get; set; }
    }
}
