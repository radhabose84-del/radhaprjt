using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities
{
    /// <summary>
    /// OCR (procurement-approval request) raised before a Purchase Order / Procurement
    /// Agreement for raw-material (cotton) procurement. Table: [Purchase].[OCREntry].
    /// </summary>
    public class OCREntry : BaseEntity
    {
        // System generated, unique, immutable (DocumentSequence / TransactionType)
        public string OcrNumber { get; set; } = default!;
        public DateTimeOffset OcrDate { get; set; }

        // ── Same-module FKs (MiscMaster) — DB constraint, Dapper JOIN on read ──
        public int ProcurementSourceId { get; set; }
        public MiscMaster ProcurementSource { get; set; } = default!;

        public int ProcurementTypeId { get; set; }
        public MiscMaster ProcurementType { get; set; } = default!;

        public int BrokerDirectId { get; set; }
        public MiscMaster BrokerDirect { get; set; } = default!;

        // Free-text broker name (no FK); null for Direct procurement
        public string? BrokerName { get; set; }

        public int? GradeId { get; set; }
        public MiscMaster? Grade { get; set; }

        // OCR processing status (Draft / Pending Approval / Approved / Rejected / Converted)
        public int StatusId { get; set; }
        public MiscMaster OcrStatus { get; set; } = default!;

        // ── Same-module FK (PaymentTermMaster) ──
        public int PaymentTermId { get; set; }
        public PaymentTermMaster PaymentTerm { get; set; } = default!;

        // ── Additional Cotton Details — same-module MiscMaster FKs (optional, no cascade) ──
        public int? PaymentModeId { get; set; }
        public MiscMaster? PaymentMode { get; set; }

        public int? WeighmentId { get; set; }           // Spot / Mill
        public MiscMaster? Weighment { get; set; }

        public int? TransitInsuranceId { get; set; }    // borne-by A/C
        public MiscMaster? TransitInsurance { get; set; }

        public int? LorryFreightId { get; set; }        // borne-by A/C
        public MiscMaster? LorryFreight { get; set; }

        public int? ModeOfTransportId { get; set; }      // Lorry / Rail / Container etc.
        public MiscMaster? ModeOfTransport { get; set; }

        // ── Additional Cotton Details — scalar fields ──
        public string? MillSampleNo { get; set; }
        public string? CottonPassedBy { get; set; }
        public decimal? GstPercentage { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? InsurancePercentage { get; set; }
        public string? Remarks { get; set; }

        // ── Cross-module FK → Inventory UOM master (rate unit, e.g. Candy/Kg). No DB constraint. ──
        public int? UomId { get; set; }

        // ── Cross-module FK → QC.QualityTemplate (optional, no DB constraint) ──
        public int? QualityTemplateId { get; set; }

        // ── Cross-module FKs — NO DB constraint, populated via lookup on read ──
        public int SupplierId { get; set; }   // Party.PartyMaster (ISupplierLookup)
        public int LocationId { get; set; }   // AppData.Location  (new location-master lookup)
        public int StationId { get; set; }    // AppData.Station   (new IStationLookup)
        public int ItemId { get; set; }       // Inventory ItemMaster (IItemLookup) — cotton type
        public int CountId { get; set; }       // Production.CountMaster (ICountMasterLookup)

        // ── Cotton commercial details ──
        public decimal Quantity { get; set; }            // Bales (label fixed, no UOM)
        public decimal? Weight { get; set; }             // Kgs
        public decimal Rate { get; set; }                // Per candy (label fixed, no UOM)
        public DateTimeOffset? ExpectedDispatchDate { get; set; }

        // Single supporting document (WhatsApp screenshot / spec / discussion copy)
        public string? DocumentPath { get; set; }

        // ── Child: dynamic cotton-quality parameter values (driven by QualityTemplateId) ──
        public ICollection<OCRQualityParameter>? OcrQualityParameters { get; set; }
    }
}
