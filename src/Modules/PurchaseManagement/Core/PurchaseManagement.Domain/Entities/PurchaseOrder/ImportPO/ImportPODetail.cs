using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities.PurchaseOrder.ImportPO;

public class ImportPODetail : IActivityTracked
{
    public int Id { get; set; } 
    public int PurchaseHeaderId { get; set; } 
    public ImportPOHeader? Header { get; set; }
    public int? IndentId { get; set; }
    public int ItemId { get; set; }
    public int ItemSno { get; set; }      
    public int UomId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }    
    public int? DutyMasterId { get; set; }    
    public DutyMaster? dutyMaster { get; set; }
    public decimal? FreightAmount { get; set; } 
    public decimal? InsuranceAmount { get; set; }
    public decimal? CIFValue { get; set; }
    public decimal? BasicCustomDuty { get; set; }
    public decimal? SocialWelfareSurCharges { get; set; }
    public decimal IGST { get; set; }
    public decimal? IGSTPercentage { get; set; }
    public decimal? AgriInfraDevCess { get; set; }
    public decimal? AntiDumpingDuty { get; set; }
    public decimal? SafeguardDuty { get; set; }
    public decimal? HealthEducationCess { get; set; }
    public decimal? OtherCharges { get; set; }
    public decimal TotalValue { get; set; }
    public bool GRBasedIV { get; set; }
    
}
