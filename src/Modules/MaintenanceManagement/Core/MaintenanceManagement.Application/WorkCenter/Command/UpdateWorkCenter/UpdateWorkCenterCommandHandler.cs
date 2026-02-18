#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.WorkCenter.Command.UpdateWorkCenter
{
    public class UpdateWorkCenterCommandHandler : IRequestHandler<UpdateWorkCenterCommand, ApiResponseDTO<int>>
    {
        
        private readonly IWorkCenterCommandRepository _iWorkCenterCommandRepository;
        private readonly IWorkCenterQueryRepository _iWorkCenterQueryRepository;
        private readonly IMapper _Imapper;
        private readonly IMediator _mediator; 
        public UpdateWorkCenterCommandHandler(IWorkCenterCommandRepository iWorkCenterCommandRepository, IWorkCenterQueryRepository iWorkCenterQueryRepository, IMapper imapper, IMediator mediator)
        {
            _iWorkCenterCommandRepository = iWorkCenterCommandRepository;
            _iWorkCenterQueryRepository = iWorkCenterQueryRepository;
            _Imapper = imapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateWorkCenterCommand request, CancellationToken cancellationToken)
        {
           // 🔹 First, check if the ID exists in the database
            // var existingworkcenter = await _iWorkCenterQueryRepository.GetByIdAsync(request.Id);
            // if (existingworkcenter is null)
            // {
        
            // return new ApiResponseDTO<int>
            // {
            //     IsSuccess = false,
            //     Message = "WorkCenter Id not found / WorkCenter is deleted ."
            // };
            // }
       
            var workCenter = _Imapper.Map<MaintenanceManagement.Domain.Entities.WorkCenter>(request);
            var result = await _iWorkCenterCommandRepository.UpdateAsync(request.Id, workCenter);
            if (result <= 0) // WorkCenter not found
            {
               
                return new ApiResponseDTO<int> { IsSuccess = false, Message = "WorkCenter not found." };
            }
            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: workCenter.WorkCenterCode,
                actionName: workCenter.WorkCenterName,
                details: $"WorkCenter details was updated",
                module: "WorkCenter");
            await _mediator.Publish(domainEvent, cancellationToken);
           
            return new ApiResponseDTO<int> { IsSuccess = true, Message = "WorkCenter Updated Successfully.", Data = result };   
        }
    }
}