
namespace FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail
{
    public class DepreciationDto 
    {
        public string? Company { get; set; }
        public string? Unit { get; set; } 
        public string? Division { get; set; } 
        public int AssetId { get; set; } 
        public string? AssetGroup { get; set; }
        public string? AssetCode { get; set; }        
        public string? AssetName { get; set; }                
        public decimal PurchaseCost { get; set; }   
        public DateTimeOffset AssetDate { get; set; }   
        public int UsefulLife { get; set; }   
        public int Residual_Per { get; set; }   
        public int ResidualValue { get; set; }   
        public int UsefulLifeDays { get; set; }   
        public int DaysOpening { get; set; }   
        public DateTimeOffset ExpiryDate { get; set; }   
        public int DaysUsedCurrent { get; set; }   
        public decimal OpeningValue { get; set; }   
        public decimal DepreciationValue { get; set; }   
        public decimal ClosingValue { get; set; }   
        public string? DepreciationPeriod { get; set; }  
        public string? DepreciationType { get; set; }  
        public DateTimeOffset StartDate { get; set; }   
        public DateTimeOffset EndDate { get; set; }   
        public decimal NetValue { get; set; } 
    }
}