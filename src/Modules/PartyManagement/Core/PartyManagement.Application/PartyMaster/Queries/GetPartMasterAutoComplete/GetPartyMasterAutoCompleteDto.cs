using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartMasterAutoComplete
{
    public class GetPartyMasterAutoCompleteDto
    {
        public int Id { get; set; }
        public string? PartyCode { get; set; }
        public string? PartyName { get; set; }
        public string? RegistrationType { get; set; }
        public string? GSTNumber { get; set; }
        public string? GSTFlag { get; set; }
        public string? PrimaryEmail { get; set; }
        public string? PrimaryMobile { get; set; } 


    }
}