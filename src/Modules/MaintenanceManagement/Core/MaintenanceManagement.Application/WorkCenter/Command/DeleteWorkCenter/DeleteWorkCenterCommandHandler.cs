#nullable disable
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.WorkCenter.Command.DeleteWorkCenter
{
    public class DeleteWorkCenterCommandHandler : IRequestHandler<DeleteWorkCenterCommand, ApiResponseDTO<int>>
    {
        private readonly IWorkCenterCommandRepository _iWorkCenterCommandRepository;
        private readonly IWorkCenterQueryRepository _iWorkCenterQueryRepository;
        private readonly IMapper _Imapper;
        private readonly IMediator _mediator; 
        public DeleteWorkCenterCommandHandler(IWorkCenterCommandRepository iWorkCenterCommandRepository, IWorkCenterQueryRepository iWorkCenterQueryRepository, IMapper imapper, IMediator mediator)
        {
            _iWorkCenterCommandRepository = iWorkCenterCommandRepository;
            _iWorkCenterQueryRepository = iWorkCenterQueryRepository;
            _Imapper = imapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(DeleteWorkCenterCommand request, CancellationToken cancellationToken)
        {
              // 🔹 First, check if the ID exists in the database
            // var existingworkcenter = await _iWorkCenterQueryRepository.GetByIdAsync(request.Id);
            // if (existingworkcenter is null)
            // {
              
            //     return new ApiResponseDTO<int>
            //     {
            //         IsSuccess = false,
            //         Message = "WorkCenter Id not found / WorkCenter is deleted ."
            //     };
            // }

            var workCenterGroup = _Imapper.Map<MaintenanceManagement.Domain.Entities.WorkCenter>(request);
            var result = await _iWorkCenterCommandRepository.DeleteAsync(request.Id,workCenterGroup);
            if (result == -1) 
            {
         
             return new ApiResponseDTO<int> { IsSuccess = false, Message = "WorkCenter not found."};
            }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: workCenterGroup.WorkCenterCode,
                actionName: workCenterGroup.WorkCenterName,
                details: $"WorkCenter details was deleted",
                module: "WorkCenter");
            await _mediator.Publish(domainEvent);
          

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,   
                Data = result,
                Message = "WorkCenter deleted successfully."
    
            };
        }
    }
}