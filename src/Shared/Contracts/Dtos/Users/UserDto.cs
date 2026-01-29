using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Contracts.Dtos.Users
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string? UserName { get; set; }
        [JsonPropertyName("emailId")]
        public string? Email { get; set; }
    }
}