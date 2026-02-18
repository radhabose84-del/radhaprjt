#nullable disable
using System.Text.Json.Serialization;

namespace Contracts.Dtos.Users
{
    public class CompanyDto
    {
        [JsonPropertyName("id")]
        public int CompanyId { get; set; }
        public string CompanyName { get; set; } = default!;
        public string LegalName { get; set; } = default!;
        public string GstNumber { get; set;  }
        public string TinNumber { get; set;  }
        public string TanNumber { get; set; } = default!;
        public string CstNumber { get; set;  }
        public int EntityId { get; set;  }

    }
}