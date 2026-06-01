namespace QCManagement.Application.QcInspection.Dto
{
    /// <summary>Context needed by the disposition handler to write back to GRN and raise the movement event.</summary>
    public class QcDispositionContextDto
    {
        public int GrnHeaderId { get; set; }
        public int GrnDetailId { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public int ReceivedUomId { get; set; }
        public string? QcInspectionNo { get; set; }
    }
}
