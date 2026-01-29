using System.Text.Json.Serialization;

namespace Contracts.Dtos.Users
{
    public class StatesDto
    {
        [JsonPropertyName("id")]
         public int StateId { get; set; }
        public string StateCode { get; set; } 
        public string StateName { get; set; } 
        public int CountryId { get; set; }
    }
}