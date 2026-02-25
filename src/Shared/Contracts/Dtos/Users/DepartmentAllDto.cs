using System.Text.Json.Serialization;

namespace Contracts.Dtos.Users
{
    public class DepartmentAllDto
    {
        [JsonPropertyName("id")]
        public int DepartmentId { get; set; }
        [JsonPropertyName("deptName")]
        public string DepartmentName { get; set; } = default!;
        public string ShortName { get; set; } = default!;
        [JsonPropertyName("DepartmentGroupId")]
        public int Departmentgroupid { get; set; }
    }
}