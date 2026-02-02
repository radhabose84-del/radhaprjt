using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPendingPo
{
    public class GetGateEntryPendingPoQuery : IRequest<List<GetGateEntryPendingPoDto>>
    {
        public int PartyId { get; set; }
    }
}