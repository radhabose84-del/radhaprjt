using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IWorkCenter;
using MaintenanceManagement.Application.WorkCenter.Queries.GetWorkCenter;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.WorkCenter.Command.CreateWorkCenter
{
    public class CreateWorkCenterCommandHandler : IRequestHandler<CreateWorkCenterCommand, ApiResponseDTO<int>>
    {
        
        private readonly IWorkCenterCommandRepository _iWorkCenterCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        public CreateWorkCenterCommandHandler(IWorkCenterCommandRepository iWorkCenterCommandRepository, IMediator imediator, IMapper imapper)
        {
            _iWorkCenterCommandRepository = iWorkCenterCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateWorkCenterCommand request, CancellationToken cancellationToken)
        {
            var workCenter = _imapper.Map<MaintenanceManagement.Domain.Entities.WorkCenter>(request);
            
            var result = await _iWorkCenterCommandRepository.CreateAsync(workCenter);

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: workCenter.WorkCenterCode,
                actionName: workCenter.WorkCenterName,
                details: $"WorkCenter details was created",
                module: "WorkCenter");
            await _imediator.Publish(domainEvent, cancellationToken);
          
            var costcenterGroupDtoDto = _imapper.Map<WorkCenterDto>(workCenter);
            if (result > 0)
                  {
                    
                        return new ApiResponseDTO<int>
                       {
                           IsSuccess = true,
                           Message = "WorkCenter created successfully",
                           Data = result
                      };
                 }
            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "WorkCenter Creation Failed",
                Data = result
            };
        }
    }
}