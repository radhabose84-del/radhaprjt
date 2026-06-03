namespace Contracts.Dtos.Lookups.Users
{
    /// <summary>
    /// Lightweight projection of the AppData.Location master (procurement source location).
    /// Distinct from <c>LocationLookupDto</c>, which is a geographic City/State/Country lookup.
    /// </summary>
    public sealed class LocationMasterLookupDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? LocationName { get; set; }
    }
}
