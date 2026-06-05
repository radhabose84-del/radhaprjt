using Contracts.Common;
using MediatR;
using QCManagement.Application.QcInspection.Commands.SaveParameterCollection;

namespace QCManagement.Application.QcInspection.Commands.SaveDisposition
{
    public class SaveDispositionCommand : IRequest<ApiResponseDTO<int>>
    {
        public int QcInspectionHdrId { get; set; }

        // Readings entered on the same screen — saved together with the disposition in one transaction.
        public List<ParameterResultInputDto> Parameters { get; set; } = new();

        public int QcStatusId { get; set; }
        public decimal AcceptedQuantity { get; set; }
        public decimal RejectedQuantity { get; set; }
        public string? DispositionRemarks { get; set; }
    }
}
