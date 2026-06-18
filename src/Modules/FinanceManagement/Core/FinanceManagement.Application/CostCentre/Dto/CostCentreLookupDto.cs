namespace FinanceManagement.Application.CostCentre.Dto
{
    public class CostCentreLookupDto
    {
        public int Id { get; set; }
        public string? CostCentreCode { get; set; }
        public string? CostCentreName { get; set; }
        public int CentreLevelId { get; set; }
    }
}
