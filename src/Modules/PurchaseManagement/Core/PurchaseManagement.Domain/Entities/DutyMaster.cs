
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.PurchaseOrder.ImportPO;

namespace PurchaseManagement.Domain.Entities
{
    public class DutyMaster : BaseEntity
    {
        public string DutyCode { get; set; } = default!;
        public string? Description { get; set; } = default!;
        public string TariffNumber { get; set; } = default!;
        public int HsnId { get; set; }       
        public string? HsnCode { get; set; }        
        public int DutyCategoryId { get; set; }
        public MiscMaster MiscDuty { get; set; } = default!;
        public decimal BasicCustomsDutyPercentage { get; set; }
        public decimal SocialWelfareSurchargePercentage { get; set; }
        public decimal IGSTPercentage { get; set; }
        public decimal? AgriInfraDevCessPercentage { get; set; }
        public decimal? AntiDumpingDutyPercentage { get; set; }
        public decimal? SafeguardDutyPercentage { get; set; }
        public decimal? HealthEducationCessPercentage { get; set; }
        public decimal? TotalDuty { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }
        public int CountryOfOriginApplicability { get; set; }
        public MiscMaster MiscCOA { get; set; } = default!;
        public string? NotificationNumber { get; set; }
        public string? Remarks { get; set; }     
        public ICollection<ImportPODetail> ImportPODuty { get; set; } = new List<ImportPODetail>();    
    }
}
