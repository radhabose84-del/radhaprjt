using Contracts.Common;
using MediatR;

namespace QCManagement.Application.QcInspection.Commands.SaveDisposition
{
    public class SaveDispositionCommand : IRequest<ApiResponseDTO<int>>
    {
        public int QcInspectionHdrId { get; set; }
        public string? QcStatusCode { get; set; }
        public decimal AcceptedQuantity { get; set; }
        public decimal RejectedQuantity { get; set; }
        public string? DispositionRemarks { get; set; }
    }
}
