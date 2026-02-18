using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.IMachineSpecification;
using MaintenanceManagement.Application.Common.Interfaces;
using MaintenanceManagement.Application.Common.Interfaces.IMachineSpecification;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineSpecification.Command.UpdateMachineSpecfication
{
    public class UpdateMachineSpecficationCommandHandler : IRequestHandler<UpdateMachineSpecficationCommand, ApiResponseDTO<bool>>
    {
        private readonly IMachineSpecificationCommandRepository _imachineSpecificationCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        private readonly IIPAddressService _ipAddressService;
        private readonly ITimeZoneService _timeZoneService;
        public UpdateMachineSpecficationCommandHandler(IMachineSpecificationCommandRepository imachineSpecificationCommandRepository, IMediator imediator, IMapper imapper, IIPAddressService ipAddressService, ITimeZoneService timeZoneService)
        {
            _imachineSpecificationCommandRepository = imachineSpecificationCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
            _ipAddressService = ipAddressService;
            _timeZoneService = timeZoneService;
        }

        public async Task<ApiResponseDTO<bool>> Handle(UpdateMachineSpecficationCommand request, CancellationToken cancellationToken)
        {
            // Map all
            var machineSpecs = _imapper.Map<List<MaintenanceManagement.Domain.Entities.MachineSpecification>>(request.Specifications);

            // Update all specs at once
            var result = await _imachineSpecificationCommandRepository.UpdateAsync(machineSpecs);

            if (result)
            {
                foreach (var spec in machineSpecs)
                {
                    var domainEvent = new AuditLogsDomainEvent(
                        actionDetail: "Update",
                        actionCode: spec.MachineId.ToString(),
                        actionName: spec.SpecificationId.ToString(),
                        details: $"MachineSpecification Updated",
                        module: "MachineSpecification");

                    await _imediator.Publish(domainEvent, cancellationToken);
                }
            }

            return new ApiResponseDTO<bool>
            {
                IsSuccess = result,
                Message = result ? "MachineSpecifications updated successfully" : "No records updated",
                Data = result
            };
     }

    }
}