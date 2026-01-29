using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace Contracts.Dtos.Maintenance
{
    public class UnitDto
    {
        [JsonPropertyName("id")]
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public string ShortName { get; set; }
        public string UnitHeadName { get; set; }
        public string OldUnitId { get; set; }
        public int? SpindlesCapacity { get; set; }
    }
}