using System.Text.Json.Serialization;

namespace Contracts.Dtos.Users
{
    public class CityDto
    {
        [JsonPropertyName("id")]
         public int CityId { get; set; }
        public string CityCode { get; set; } = default!;
        public string CityName { get; set; } = default!;
        public int StateId { get; set; }
    }
}