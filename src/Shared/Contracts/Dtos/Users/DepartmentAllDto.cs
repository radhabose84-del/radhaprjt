using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Contracts.Dtos.Users
{
    public class DepartmentAllDto
    {
        [JsonPropertyName("id")]
        public int DepartmentId { get; set; }
        [JsonPropertyName("deptName")]
        public string DepartmentName { get; set; }
        public string ShortName { get; set; }
        [JsonPropertyName("DepartmentGroupId")]
        public int Departmentgroupid { get; set; }
    }
}