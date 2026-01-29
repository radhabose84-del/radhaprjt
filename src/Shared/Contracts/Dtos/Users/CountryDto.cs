using System.Text.Json.Serialization;

namespace Contracts.Dtos.Users
{
    public class CountryDto
    {
        [JsonPropertyName("id")]
         public int CountryId { get; set; }
        public string CountryCode { get; set; } 
        public string CountryName { get; set; }         
    }
}