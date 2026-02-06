using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartMaster
{
    public class GetPartyMasterDto
    {
        public int Id { get; set; }
        public string PartyCode { get; set; } = string.Empty;
        public string PartyName { get; set; } = string.Empty;
        public string RegistrationType { get; set; } = string.Empty;
        public string GSTNumber { get; set; } = string.Empty;
        public string PAN { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Party_GroupType { get; set; } = string.Empty;
        public string PartyStatus { get; set; } = string.Empty;
        public DateTimeOffset CreatedDate { get; set; }
    }
}