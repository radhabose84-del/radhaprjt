using Contracts.Common;
using MediatR;
using SalesManagement.Application.DeliveryChallan.Dto;

namespace SalesManagement.Application.DeliveryChallan.Commands.GenerateEWaybillForDC
{
    public sealed record GenerateEWaybillForDCCommand(int DeliveryChallanId)
        : IRequest<ApiResponseDTO<GenerateEWaybillResponseDto>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
