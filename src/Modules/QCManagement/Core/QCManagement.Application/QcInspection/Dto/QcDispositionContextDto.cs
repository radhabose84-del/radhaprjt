namespace QCManagement.Application.QcInspection.Dto
{
    /// <summary>Context needed by the disposition handler to write back to GRN and raise the movement event.</summary>
    public class QcDispositionContextDto
    {
        public int SourceTypeId { get; set; }
        public int SourceHeaderId { get; set; }
        public int SourceDetailId { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public int ReceivedUomId { get; set; }
        public string? QcInspectionNo { get; set; }
    }
}
