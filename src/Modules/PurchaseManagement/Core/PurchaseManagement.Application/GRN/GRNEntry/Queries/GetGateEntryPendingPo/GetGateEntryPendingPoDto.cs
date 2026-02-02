using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPendingPo
{
    public class GetGateEntryPendingPoDto
    {
        public int GateEntryId { get; set; }
        public int PoId { get; set; }
        public string? PONumber { get; set; }
        public DateTimeOffset PoDate { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
    }
}