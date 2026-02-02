using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.PartyMaster.Queries.GetPartyDetails
{
    public class GetPartyDetailsQuery  : IRequest<List<PartyMasterDTO>>
    {
        public string? OldunitCode { get; set; }
        public string? SearchPattern { get; set; }
    }
}