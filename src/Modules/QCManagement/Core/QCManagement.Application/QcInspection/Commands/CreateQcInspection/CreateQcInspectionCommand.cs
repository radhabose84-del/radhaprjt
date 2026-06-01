using Contracts.Common;
using MediatR;

namespace QCManagement.Application.QcInspection.Commands.CreateQcInspection
{
    public class CreateQcInspectionCommand : IRequest<ApiResponseDTO<int>>
    {
        public int GrnDetailId { get; set; }
    }
}
