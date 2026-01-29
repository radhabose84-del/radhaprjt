using System.Text.Json.Serialization;

namespace Contracts.Dtos.Users
{
    public class CompanyDto
    {
        [JsonPropertyName("id")]
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string LegalName { get; set; }
        public string GstNumber { get; set;  }
        public string TinNumber { get; set;  }
        public string TanNumber { get; set; } 
        public string CstNumber { get; set;  }
        public int EntityId { get; set;  }

    }
}