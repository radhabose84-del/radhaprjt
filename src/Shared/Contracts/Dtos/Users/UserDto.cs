using System.Text.Json.Serialization;

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