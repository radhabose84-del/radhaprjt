#nullable disable

using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.BusinessUnit.Commands.UpdateBusinessUnit
{
    public class UpdateBusinessUnitCommandHandler : IRequestHandler<UpdateBusinessUnitCommand, ApiResponseDTO<int>>
    {
        private readonly IBusinessUnitCommandRepository _commandRepository;
        private readonly IBusinessUnitQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateBusinessUnitCommandHandler(
            IBusinessUnitCommandRepository commandRepository,
            IBusinessUnitQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateBusinessUnitCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.BusinessUnit>(request);
            entity.IsActive = request.IsActive == 1
                ? Domain.Common.BaseEntity.Status.Active
                : Domain.Common.BaseEntity.Status.Inactive;

            var updatedId = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "BUSINESSUNIT_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Business Unit with Id {request.Id} updated successfully.",
                module: "BusinessUnit"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Business Unit updated successfully.",
                Data = updatedId
            };
        }
    }
}
