using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PartyManagement.Application.Common.Mappings;
using PartyManagement.Domain.Entities;

namespace PartyManagement.Application.PartyMaster.Command.UploadPartyMasterDocument
{
    public class PartyDocumetDto  : IMapFrom<PartyDocument>
    {
        public string? FileName { get; set; }
        public string? PartyDocumentBase64 { get; set; }
    }
}