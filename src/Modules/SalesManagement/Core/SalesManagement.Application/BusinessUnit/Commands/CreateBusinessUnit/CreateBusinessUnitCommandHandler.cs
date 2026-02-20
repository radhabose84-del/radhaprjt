#nullable disable

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

        public CreateBusinessUnitCommandHandler(
            IBusinessUnitCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateBusinessUnitCommand request, CancellationToken cancellationToken)
        {
            var businessUnit = new Domain.Entities.BusinessUnit
            {
                BusinessUnitCode = request.BusinessUnitCode,
                BusinessUnitName = request.BusinessUnitName,
                Description = request.Description
            };

            var newId = await _commandRepository.CreateAsync(businessUnit);

            // Publish audit log event
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
                Message = "Business Unit created successfully",
                Data = newId
            };
        }
    }
}
