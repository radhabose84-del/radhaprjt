using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Contracts.Dtos.Users
{
    public class CityDto
    {
        [JsonPropertyName("id")]
         public int CityId { get; set; }
        public string CityCode { get; set; } 
        public string CityName { get; set; } 
        public int StateId { get; set; }
    }
}