using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.IMachineSpecification;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineSpecification.DeleteMachineSpecfication
{
    public class DeleteMachineSpecficationCommandHandler : IRequestHandler<DeleteMachineSpecficationCommand, ApiResponseDTO<int>>
    {
        private readonly IMachineSpecificationCommandRepository _imachineSpecificationCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

        public DeleteMachineSpecficationCommandHandler(IMachineSpecificationCommandRepository imachineSpecificationCommandRepository, IMediator imediator, IMapper imapper)
        {
            _imachineSpecificationCommandRepository = imachineSpecificationCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(DeleteMachineSpecficationCommand request, CancellationToken cancellationToken)
        {
            var machineMaster = _imapper.Map<MaintenanceManagement.Domain.Entities.MachineSpecification>(request);
            var result = await _imachineSpecificationCommandRepository.DeleteAsync(request.Id,machineMaster);
            if (result == -1) 
            {
         
             return new ApiResponseDTO<int> { IsSuccess = false, Message = "MachineSpecificationId not found."};
            }

            //Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: machineMaster.Id.ToString(),
                actionName: machineMaster.SpecificationId.ToString(),
                details: $"MachineSpecification details was deleted",
                module: "MachineSpecification");
            await _imediator.Publish(domainEvent);
            return new ApiResponseDTO<int>
            {
                IsSuccess = true,   
                Data = result,
                Message = "MachineSpecification deleted successfully."
    
            };
        }
        
    }
}