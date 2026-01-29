
namespace FAM.Application.WDVDepreciation.Queries.GetDepreciation
{
    public class CalculationDepreciationDto
    {
        public string? GroupName { get; set; }
        public string? SubGroupName { get; set; }
        public int AssetGroupId { get; set; }
        public int? AssetSubGroupId { get; set; }
        public int FinYear { get; set; }
        public int FinYearId { get; set; }
        public decimal Percentage { get; set; }
        public decimal OpeningValue { get; set; }
        public decimal AdditionLastYear { get; set; }
        public decimal MoreThan180DaysValue { get; set; }
        public decimal LessThan180DaysValue { get; set; }
        public decimal DeletionValue { get; set; }
        public decimal ClosingValue { get; set; }
        public decimal DepreciationValue { get; set; }
        public decimal NormalDepreciation { get; set; }
        public decimal AdditionalDepreciation { get; set; }
        public decimal AdditionalCarryForward { get; set; }
        public decimal WDVDepreciationValue { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
    }
}