namespace Core.Application.Country.Queries.GetCountries
{
    public class CountryAutoCompleteDTO
    {
        public int Id { get; set; }
        public string? CountryCode { get; set; }
        public string? CountryName { get; set; } 
    }
}