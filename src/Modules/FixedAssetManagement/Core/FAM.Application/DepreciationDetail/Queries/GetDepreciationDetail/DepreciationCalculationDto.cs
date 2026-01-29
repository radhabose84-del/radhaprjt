
namespace FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail
{
    public class DepreciationCalculationDto
    {        
        public int companyId { get; set; }
        public int unitId { get; set; } 
        public int finYearId { get; set; } 
        public DateTimeOffset? startDate { get; set; }   
        public DateTimeOffset? endDate { get; set; }   
        public int depreciationType { get; set; }
    }
}