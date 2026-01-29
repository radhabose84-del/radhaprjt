namespace FAM.Application.DepreciationDetail.Queries.GetDepreciationDetail
{
    public class DepreciationAbstractDto
    {
        public string? Company { get; set; }
        public string? Unit { get; set; } 
        public string? Division { get; set; } 
        public string? AssetGroup { get; set; }
        public decimal GrossBlockOpening { get; set; }
        public decimal Addition { get; set; }
        public decimal Deletion { get; set; }
        public decimal GrossBlockClosing { get; set; }
        public decimal DepreciationOpening { get; set; }
        public decimal DepreciationCurrent { get; set; }
        public decimal DisposalDepreciation { get; set; }
        public decimal DepreciationClosing { get; set; }
        public decimal NetBlockClosing { get; set; }
        public decimal NetBlockOpening { get; set; }
        public DateTimeOffset startDate { get; set; }
        public DateTimeOffset endDate { get; set; }
        public string? DepType { get; set; }
        public string? DepPeriod { get; set; }
    }
}