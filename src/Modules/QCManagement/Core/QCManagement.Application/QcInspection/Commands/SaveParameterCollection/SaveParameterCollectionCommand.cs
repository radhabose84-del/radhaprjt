using Contracts.Common;
using MediatR;

namespace QCManagement.Application.QcInspection.Commands.SaveParameterCollection
{
    public class SaveParameterCollectionCommand : IRequest<ApiResponseDTO<int>>
    {
        public int QcInspectionHdrId { get; set; }
        public List<ParameterResultInputDto> Parameters { get; set; } = new();
    }

    public class ParameterResultInputDto
    {
        public int DetailId { get; set; }
        public string? ActualValue { get; set; }
        public string? Remarks { get; set; }
    }
}
