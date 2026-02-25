using System.Text.Json.Serialization;


namespace Contracts.Dtos.Maintenance
{
    public class UnitDto
    {
        [JsonPropertyName("id")]
        public int UnitId { get; set; }
        public string UnitName { get; set; } = default!;
        public string ShortName { get; set; } = default!;
        public string UnitHeadName { get; set; } = default!;
        public string OldUnitId { get; set; } = default!;
        public int? SpindlesCapacity { get; set; }
    }
}