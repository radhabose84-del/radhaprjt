using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MaintenanceManagement.Application.Common.HttpResponse;
using MaintenanceManagement.Application.Common.Interfaces.IMachineSpecification;
using MaintenanceManagement.Application.MachineSpecification.Command;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.Application.MachineSpecification.Queries.GetMachineSpecificationById
{
    public class GetMachineSpecificationByIdQueryHandler : IRequestHandler<GetMachineSpecificationByIdQuery, ApiResponseDTO<List<MachineSpecificationDto>>>
    {
        private readonly IMachineSpecificationQueryRepository _imachineSpecificationQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMachineSpecificationByIdQueryHandler(IMachineSpecificationQueryRepository imachineSpecificationQueryRepository, IMapper mapper, IMediator mediator)
        {
            _imachineSpecificationQueryRepository = imachineSpecificationQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<MachineSpecificationDto>>> Handle(GetMachineSpecificationByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _imachineSpecificationQueryRepository.GetByIdAsync(request.Id);

            if (result == null || !result.Any())
            {
                return new ApiResponseDTO<List<MachineSpecificationDto>>
                {
                    IsSuccess = false,
                    Message = $"No MachineSpecifications found for Machine ID {request.Id}"
                };
            }

            // If needed: map again using AutoMapper (optional)
             var mappedList = _mapper.Map<List<MachineSpecificationDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetMachineSpecificationByIdQuery",
                actionName: request.Id.ToString(),
                details: $"Fetched {result.Count} MachineSpecifications for Machine ID {request.Id}.",
                module: "MachineSpecification");

            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<MachineSpecificationDto>>
            {
                IsSuccess = true,
                Message = "Machine specifications fetched successfully.",
                Data = result
            };
        }
    
    }
}