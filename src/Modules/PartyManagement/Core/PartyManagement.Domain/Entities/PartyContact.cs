using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyManagement.Domain.Entities
{
    public class PartyContact
    {
        public int Id { get; set; }   // PK
        public int PartyId { get; set; }     // FK to PartyMaster
        public PartyMaster PartyContactId { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? GenderId { get; set; }
        public MiscMaster? Gender { get; set; } = null!;
        public string? Designation { get; set; }
        public string? EmailID { get; set; }
        public string? MobileNo { get; set; }
        public string? Phone { get; set; }
        public int? PreferredChannelId { get; set; }
        public MiscMaster? PreferredChannel { get; set; } = null!;
        public int? ContactTypeId { get; set; }
        public MiscMaster? ContactType { get; set; } = null!;
        public string? ContactBy { get; set; }
    }
}