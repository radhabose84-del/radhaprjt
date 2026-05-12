using System.Text.Json.Serialization;

namespace FinanceManagement.Application.EInvoiceHeader.Dto
{
    // Payload for NIC standalone e-Waybill API (Generate without IRN).
    // Used by Delivery Challan, Bill of Supply, etc. — anything not backed by an e-Invoice.
    // Field names mirror NIC v1.03 ewayapi schema (lowerCamelCase) — do NOT rename
    // without verifying against NIC sandbox or the field will be silently ignored.
    public class StandaloneEwbPayloadDto
    {
        // Supply / document classification
        public string SupplyType { get; set; } = "O";       // O=Outward, I=Inward
        public string SubSupplyType { get; set; } = "5";    // 1=Supply, 5=For Own Use, etc.
        public string? SubSupplyDesc { get; set; }
        public string DocType { get; set; } = "CHL";        // CHL=Delivery Challan, INV=Tax Invoice, BIL=Bill of Supply
        public string DocNo { get; set; } = string.Empty;
        public string DocDate { get; set; } = string.Empty; // dd/MM/yyyy
        public int TransactionType { get; set; } = 1;       // 1=Regular, 2=Bill To, 3=Bill From, 4=Combination

        // Consignor (From)
        public string FromGstin { get; set; } = string.Empty;
        public string FromTrdName { get; set; } = string.Empty;
        public string? FromAddr1 { get; set; }
        public string? FromAddr2 { get; set; }
        public string FromPlace { get; set; } = string.Empty;
        public int FromPincode { get; set; }
        public int FromStateCode { get; set; }
        public int ActFromStateCode { get; set; }
        // Hint for orchestrator-side enrichment when FromAddr1/Place/Pincode are blank.
        // Not serialized to NIC — internal use only.
        [JsonIgnore] public int? FromUnitId { get; set; }

        // Consignee (To)
        public string ToGstin { get; set; } = string.Empty;
        public string ToTrdName { get; set; } = string.Empty;
        public string? ToAddr1 { get; set; }
        public string? ToAddr2 { get; set; }
        public string ToPlace { get; set; } = string.Empty;
        public int ToPincode { get; set; }
        public int ToStateCode { get; set; }
        public int ActToStateCode { get; set; }
        [JsonIgnore] public int? ToUnitId { get; set; }

        // Values
        public decimal TotalValue { get; set; }
        public decimal CgstValue { get; set; }
        public decimal SgstValue { get; set; }
        public decimal IgstValue { get; set; }
        public decimal CessValue { get; set; }
        public decimal CessNonAdvolValue { get; set; }
        public decimal OtherValue { get; set; }
        public decimal TotInvValue { get; set; }

        // Transport
        public string? TransMode { get; set; }              // 1=Road, 2=Rail, 3=Air, 4=Ship
        public int TransDistance { get; set; }
        public string? TransporterName { get; set; }
        public string? TransporterId { get; set; }          // GSTIN of transporter
        public string? TransDocNo { get; set; }
        public string? TransDocDate { get; set; }           // dd/MM/yyyy
        public string? VehicleNo { get; set; }
        public string? VehicleType { get; set; } = "R";     // R=Regular, O=Over-dimensional

        // Items
        public List<StandaloneEwbItemDto> ItemList { get; set; } = new();
    }

    public class StandaloneEwbItemDto
    {
        public int ItemNo { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductDesc { get; set; }
        public int HsnCode { get; set; }
        public decimal Quantity { get; set; }
        public string QtyUnit { get; set; } = "NOS";        // NIC UQC code
        public decimal CgstRate { get; set; }
        public decimal SgstRate { get; set; }
        public decimal IgstRate { get; set; }
        public decimal CessRate { get; set; }
        public decimal CessNonAdvol { get; set; }
        public decimal TaxableAmount { get; set; }
    }
}
