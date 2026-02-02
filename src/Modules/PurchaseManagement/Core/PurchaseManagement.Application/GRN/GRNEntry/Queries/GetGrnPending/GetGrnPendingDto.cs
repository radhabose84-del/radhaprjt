using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPending
{
    public class GetGrnPendingDto
    {
        public int GateEntryId { get; set; }
        public int UnitId { get; set; }
        public int PartyId { get; set; }
        public string? GateEntryNo { get; set; }
        public DateTimeOffset? GateEntryDate { get; set; }
        public byte IsPartialReceiptAllowed { get; set; }
        public List<GetGrnPendingDetailsGateDto> GrnDetails { get; set; } = new();

        public class GetGrnPendingDetailsGateDto
        {
                public int PoId { get; set; }
                public string? PONumber { get; set; }
                public int POCategoryId { get; set; }
                public int POMethodId { get; set; }
                public int PoSlNo { get; set; }
                public int ItemId { get; set; }
                public string? ItemCode { get; set; }
                public string? ItemName { get; set; }
                public string? UOMName { get; set; }
                public decimal? UpperTolerance { get; set; }
                public decimal? LowerTolerance { get; set; }
                public decimal? OrderQuantity { get; set; }
                public decimal? TotalGrnQty { get; set; }
                public decimal? PendingQty { get; set; }
        }
        
    

    }
}