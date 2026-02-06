using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyManagement.Application.PartyMaster.Command.CreatePartyMaster
{
    public class RegistrationDto
    {
        public int RegistrationTypeId { get; set; }
        public string? Description { get; set; }
    }
}