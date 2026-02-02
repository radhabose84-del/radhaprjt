using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.GRN.GateEntry.Queries.GetGateEntriesApprovedPo
{
    public class GetGateEntriesApprovedPoQuery : IRequest<List<GetGateEntriesApprovedPoDto>>
    {
        public int PartyId { get; set; }
    }
}