using FAM.Domain.Common;

namespace FAM.Domain.Entities
{
    public class DepreciationDetails : BaseEntity
    {        
        public int CompanyId { get; set; } 
        public int UnitId { get; set; } 
        public int? Finyear { get; set; }        
        public DateTimeOffset? StartDate { get; set; }    
        public DateTimeOffset? EndDate { get; set; }                    
        public int DepreciationType { get; set; }
        public MiscMaster DepType { get; set; } = null!;  
        public int? AssetId { get; set; }          
        public AssetMasterGenerals AssetMasterId { get; set; } = null!;      
        // Foreign Key
        public int AssetGroupId { get; set; }
        public AssetGroup AssetGroup { get; set; } = null!;    
        public decimal AssetValue { get; set; }
        public DateTimeOffset? CapitalizationDate { get; set; }                       
        public decimal ResidualValue { get; set; }
        public DateTimeOffset? ExpiryDate { get; set; }   
        public int UsefulLifeDays   { get; set; }   
        public int DaysOpening { get; set; }   
        public int DaysUsed { get; set; }   
        public decimal OpeningValue { get; set; }   
        public decimal DepreciationValue { get; set; }   
        public decimal ClosingValue { get; set; }   
        public decimal NetValue { get; set; }   
        public byte IsLocked { get; set; }
        public int? DepreciationPeriod { get; set; }        
        public MiscMaster DepMiscType { get; set; } = null!;  
        public DateTimeOffset? DisposedDate { get; set; }    
        public decimal? DisposalAmount { get; set; }   

    }
}