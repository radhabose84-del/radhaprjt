using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.BusinessUnit.Commands.CreateBusinessUnit
{
    public class CreateBusinessUnitCommandHandler : IRequestHandler<CreateBusinessUnitCommand, ApiResponseDTO<int>>
    {
        private readonly IBusinessUnitCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateBusinessUnitCommandHandler(
            IBusinessUnitCommandRepository commandRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateBusinessUnitCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.BusinessUnit>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "BUSINESSUNIT_CREATE",
                actionName: request.BusinessUnitCode,
                details: $"Business Unit '{request.BusinessUnitCode}' created successfully with Id {newId}.",
                module: "BusinessUnit"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Business Unit created successfully.",
                Data = newId
            };
        }
    }
}
