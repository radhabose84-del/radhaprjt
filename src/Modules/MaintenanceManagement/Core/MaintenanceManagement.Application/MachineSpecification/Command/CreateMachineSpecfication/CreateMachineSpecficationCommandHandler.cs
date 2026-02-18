#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Contracts.Common;
using MaintenanceManagement.Application.Common.IMachineSpecification;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineSpecification.Command.CreateMachineSpecfication
{
    public class CreateMachineSpecficationCommandHandler  : IRequestHandler<CreateMachineSpecficationCommand, ApiResponseDTO<List<int>>>
    {
        private readonly IMachineSpecificationCommandRepository _imachineSpecificationCommandRepository;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;

        public CreateMachineSpecficationCommandHandler(IMachineSpecificationCommandRepository imachineSpecificationCommandRepository, IMediator imediator, IMapper imapper)
        {
            _imachineSpecificationCommandRepository = imachineSpecificationCommandRepository;
            _imediator = imediator;
            _imapper = imapper;
        }

         public async Task<ApiResponseDTO<List<int>>> Handle(CreateMachineSpecficationCommand request, CancellationToken cancellationToken)
        {
            var ids = new List<int>();

          foreach (var item in request.Specifications)
          {
            var machineSpec = _imapper.Map<MaintenanceManagement.Domain.Entities.MachineSpecification>(item);

            var result = await _imachineSpecificationCommandRepository.CreateAsync(machineSpec);

            // Raise domain event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: machineSpec.MachineId.ToString(),
                actionName: machineSpec.SpecificationId.ToString(),
                details: $"MachineSpecification created",
                module: "MachineSpecification");

            await _imediator.Publish(domainEvent, cancellationToken);

            if (result > 0)
            {
                ids.Add(result);
            }
        }

        return new ApiResponseDTO<List<int>>
        {
            IsSuccess = ids.Count > 0,
            Message = ids.Count > 0 ? "MachineSpecifications created successfully" : "No records inserted",
            Data = ids
        };
    }
    }
}