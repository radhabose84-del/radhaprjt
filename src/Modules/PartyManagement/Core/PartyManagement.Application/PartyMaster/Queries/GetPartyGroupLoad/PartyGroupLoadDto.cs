using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartyGroupLoad
{
    public class PartyGroupLoadDto
    {
        public int GroupId { get; set; }
        public string? PartyGroupName { get; set; }
        public int PartyTypeId { get; set; }
        public string? PartyTypeName { get; set; }
        

    }
}