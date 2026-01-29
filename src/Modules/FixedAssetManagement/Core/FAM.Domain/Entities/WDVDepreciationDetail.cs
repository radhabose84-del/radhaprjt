using FAM.Domain.Common;

namespace FAM.Domain.Entities
{
    public class WDVDepreciationDetail : BaseEntity
    {        
        public int CompanyId { get; set; }         
        public int? FinYear { get; set; }        
        public DateTimeOffset? StartDate { get; set; }    
        public DateTimeOffset? EndDate { get; set; }                    
        // Foreign Key
        public int AssetGroupId { get; set; }
        public AssetGroup AssetGroup { get; set; } = null!;            
        public int? AssetSubGroupId { get; set; }
        public AssetSubGroup? AssetSubGroup { get; set; } = null!;    
        public decimal DepreciationPercentage { get; set; }
        public decimal OpeningValue { get; set; }
        public decimal LastYearAdditionalDepreciation { get; set; }
        public decimal LessThan180DaysValue { get; set; }
        public decimal MoreThan180DaysValue { get; set; }
        public decimal DeletionValue { get; set; }
        public decimal ClosingValue { get; set; }
        public decimal DepreciationValue { get; set; }
        public decimal AdditionalDepreciationValue { get; set; }
        public decimal WDVDepreciationValue { get; set; }
        public decimal AdditionalCarryForward  { get; set; }
        public decimal CapitalGainLossValue { get; set; }
        public byte IsLocked { get; set; }
    }
}