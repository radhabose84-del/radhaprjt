using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Events.Users
{
    public class PartySyncIntegrationEvent
    {    

        public Guid CorrelationId { get; set; }
        public int PartyId { get; set; }

        // Mirror current state from Party
        public bool IsPortalAccessEnabled { get; set; }

        public IReadOnlyList<int>? CompanyIds { get; set; } = Array.Empty<int>();
        public IReadOnlyList<int>? UnitIds { get; set; } = Array.Empty<int>();

        // Re-map roles on the user side (e.g., SUPPLIER/CUSTOMER/AGENT)
        public IReadOnlyList<string>? PartyTypeCodes { get; set; } = Array.Empty<string>();

        // Optional defaults (handy on first create or when lists are empty)
        public int? DefaultCompanyId { get; set; }
        public int? DefaultUnitId { get; set; }

        // Optional identity/contact sync
        public string? PartyName { get; set; }
        public string? PartyLastName { get; set; }
        public string? Email { get; set; }
        public string? Mobile { get; set; }

        public int CreatedBy { get; set; }
        public int CreatedByName { get; set; }
        public  DateTime CreatedAt { get; set;}
        public string CreatedIp { get; set; }
        // Audit/meta
        public int?  ModifiedBy   { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIp { get; set; }
        public DateTime? ModifiedAt { get; set; }

        // Event versioning/idempotency
        public int Version { get; set; } = 1;
        public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
        public string? IdempotencyKey { get; set; } // optional if you want extra safety

        

    }
}