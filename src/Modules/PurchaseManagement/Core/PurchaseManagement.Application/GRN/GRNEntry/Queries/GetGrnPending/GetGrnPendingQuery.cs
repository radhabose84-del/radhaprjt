using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPending
{
    public class GetGrnPendingQuery : IRequest<List<GetGrnPendingDto>>
    {
        public int PartyId { get; set; }
        public int PoId { get; set; }
        public int GateEntryId { get; set; }
        
    }
}