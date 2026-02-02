using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.PurchaseOrder;

namespace PurchaseManagement.Domain.Entities.PurchaseOrder.ImportPO;

public class ImportPOHeader : BaseEntity, IActivityTracked
{
    public int PurchaseOrderId { get; set; }
    public PurchaseOrderHeader? ImportPurchase { get; set; }
    public int? TTExchangeRateId { get; set; }
    public ExchangeRate? EXRate { get; set; }
    public decimal? TTExchangeRate { get; set; }
    public int IncotermId { get; set; }
    public MiscMaster? MiscIncoterms { get; set; }
    public int? ShippingPortId { get; set; }
    public PortMaster? ShipPort { get; set; }
    public int? DestinationPortId { get; set; }
    public PortMaster? DestPort { get; set; }
    public int ModeOfTransportId { get; set; }
    public MiscMaster? MOT { get; set; }
    public int? ShippingModeId { get; set; }
    public MiscMaster? ShippingMode { get; set; }
    public int? CustomsOfficeId { get; set; }
    public MiscMaster? CustomsOffice { get; set; }
    public int? OriginCountryId { get; set; }
    public int? InsuranceProviderId { get; set; }
    public int? FreightForwarderId { get; set; }
    public int? FreeDaysAllowed { get; set; }
    public string? DemurrageTerms { get; set; }
    public string? BillOfLadingNumber { get; set; }
    public string? VesselName { get; set; }
    public string? ContainerNumber { get; set; }
    public string? AirlineName { get; set; }
    public string? AirWaybillNumber { get; set; }
    public DateTimeOffset? AirWaybillDate { get; set; }
    public string? FlightNumber { get; set; }
    public DateTimeOffset ExpectedTimeOfDeparture { get; set; }
    public DateTimeOffset ExpectedTimeOfArrival { get; set; }
    public int? CustomsHouseAgentId { get; set; }
    public string? BillOfEntryNumber { get; set; } = default!;
    public int? LCPaymentModeId { get; set; }
    public MiscMaster? LCPaymentMode { get; set; }
    public int? LCPaymentStatusId { get; set; }
    public MiscMaster? LCPaymentStatus { get; set; }
    public string? LCNumber { get; set; }
    public DateTimeOffset? LCDate { get; set; }
    public DateTimeOffset? LCExpiryDate { get; set; }
    public decimal? LCAmount { get; set; }
    public int? LCCurrencyId { get; set; }
    public int? LCIssueBankId { get; set; }
    public int? LCBeneficiaryBankId { get; set; }
    public int? LCTypeId { get; set; }
    public MiscMaster? LCType { get; set; }
    public string? LCRemarks { get; set; }
    public string? TTReferenceNumber { get; set; }
    public DateTimeOffset? TTTransferDate { get; set; }
    public int? TTBankId { get; set; }
    public int? TTCurrencyId { get; set; }
    public int? TTPaymentModeId { get; set; }
    public MiscMaster? TTPaymentMode { get; set; }
    public int? TTPaymentStatusId { get; set; }
    public MiscMaster? TTPaymentStatus { get; set; }
    public string? TTRemarks { get; set; }    
    public string? TTSwiftCode { get; set; }
    public string? LCSwiftCode { get; set; }
    public bool IsPartialReceiptAllowed { get; set; }
    public ICollection<ImportPODetail> ImportPODetails { get; set; } = new List<ImportPODetail>();     
}
