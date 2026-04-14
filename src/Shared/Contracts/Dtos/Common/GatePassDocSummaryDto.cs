namespace Contracts.Dtos.Common
{
    public sealed class GatePassDocSummaryDto
    {
        public int DocId { get; set; }
        public decimal? TotalQty { get; set; }
        public decimal? NetKgs { get; set; }
        public decimal? GrossKgs { get; set; }
        public decimal? WithLoadKgs { get; set; }
        public decimal? WithoutLoadKgs { get; set; }
        public decimal? TotalWeight { get; set; }
        public string? TransporterName { get; set; }
        public string? ItemDescription { get; set; }
        public string? Uom { get; set; }
    }
}
