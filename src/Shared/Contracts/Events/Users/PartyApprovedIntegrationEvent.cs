using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Events.Users
{
    public class PartyApprovedIntegrationEvent
    {
        public Guid CorrelationId { get; set; }
        public int PartyId { get; set; }
        public IReadOnlyList<int> CompanyIds { get; set; } = new List<int>();
        public IReadOnlyList<int> UnitIds { get; set; } = new List<int>();

        public string PartyCode { get; set; } 
        public string PartyName { get; set; }
        public string PartyLastName { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }

        public   bool IsPortalAccessEnabled { get; set; }

        // defaults (optional)
        public int? DefaultRoleId { get; set; }
        public int? DefaultCompanyId { get; set; }
        public int? DefaultUnitId { get; set; }
        public string CreatedByName { get; set; }
        public int CreatedBy { get; set; }
        public  DateTime CreatedAt { get; set;}
        public string CreatedIp { get; set; }

        public int?  ModifiedBy   { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIp { get; set; }
        public DateTime? ModifiedAt { get; set; }


        public IReadOnlyList<string> PartyTypeCodes { get; set; } = new List<string>();
    }
}