#nullable disable

using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.BusinessUnit.Commands.UpdateBusinessUnit
{
    public class UpdateBusinessUnitCommandHandler : IRequestHandler<UpdateBusinessUnitCommand, ApiResponseDTO<int>>
    {
        private readonly IBusinessUnitCommandRepository _commandRepository;
        private readonly IBusinessUnitQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public UpdateBusinessUnitCommandHandler(
            IBusinessUnitCommandRepository commandRepository,
            IBusinessUnitQueryRepository queryRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateBusinessUnitCommand request, CancellationToken cancellationToken)
        {
            var existing = await _queryRepository.GetByIdAsync(request.Id);

            if (existing == null)
                throw new EntityNotFoundException("Business Unit not found");

            var businessUnit = new Domain.Entities.BusinessUnit
            {
                Id = request.Id,
                BusinessUnitCode = existing.BusinessUnitCode, // Code is immutable
                BusinessUnitName = request.BusinessUnitName,
                Description = request.Description,
                IsActive = request.IsActive == 1 ? Status.Active : Status.Inactive
            };

            var updatedId = await _commandRepository.UpdateAsync(businessUnit);

            // Publish audit log event
            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "BUSINESSUNIT_UPDATE",
                actionName: existing.BusinessUnitCode,
                details: $"Business Unit '{existing.BusinessUnitCode}' updated successfully.",
                module: "BusinessUnit"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Business Unit updated successfully",
                Data = updatedId
            };
        }
    }
}
