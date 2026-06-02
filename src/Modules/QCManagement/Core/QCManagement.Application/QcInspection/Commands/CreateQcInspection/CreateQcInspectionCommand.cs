using Contracts.Common;
using MediatR;
using QCManagement.Application.QcInspection.Dto;

namespace QCManagement.Application.QcInspection.Commands.CreateQcInspection
{
    public class CreateQcInspectionCommand : IRequest<ApiResponseDTO<QcInspectionDto>>
    {
        public int GrnDetailId { get; set; }
    }
}
