namespace Contracts.Dtos.Lookups.Users
{
    /// <summary>
    /// Lightweight projection of the AppData.Station master (dispatch station).
    /// </summary>
    public sealed class StationLookupDto
    {
        public int Id { get; set; }
        public string? Code { get; set; }
        public string? StationName { get; set; }
    }
}
