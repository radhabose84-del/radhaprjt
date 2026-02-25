using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestStatusCommand
{
    public class UpdateMaintenanceRequestStatusCommandHandler : IRequestHandler<UpdateMaintenanceRequestStatusCommand, ApiResponseDTO<bool>>
    {
        

        private readonly IMaintenanceRequestCommandRepository _maintenanceRequestCommandRepository;

         public UpdateMaintenanceRequestStatusCommandHandler(IMaintenanceRequestCommandRepository maintenanceRequestCommandRepository)
            {
                _maintenanceRequestCommandRepository = maintenanceRequestCommandRepository;
            }

            public async Task<ApiResponseDTO<bool>> Handle(UpdateMaintenanceRequestStatusCommand request, CancellationToken cancellationToken)
            {
               // var updated = await _maintenanceRequestCommandRepository.UpdateStatusAsync(request.Id, request.RequestStatusId);
               var updated = await _maintenanceRequestCommandRepository.UpdateStatusAsync(request.Id);
                return new ApiResponseDTO<bool>
                {
                    IsSuccess = updated,
                    Message = updated ? "Status updated successfully." : "Update failed.",
                    Data = updated
                };
            }
    }
}