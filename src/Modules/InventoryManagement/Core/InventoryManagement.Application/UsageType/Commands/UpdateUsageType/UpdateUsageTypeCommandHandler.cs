using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IUsageType;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.UsageType.Commands.UpdateUsageType
{
    public class UpdateUsageTypeCommandHandler : IRequestHandler<UpdateUsageTypeCommand, ApiResponseDTO<int>>
    {
        private readonly IUsageTypeCommandRepository _commandRepository;
        private readonly IUsageTypeQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateUsageTypeCommandHandler(
            IUsageTypeCommandRepository commandRepository,
            IUsageTypeQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateUsageTypeCommand request, CancellationToken cancellationToken)
        {
            if (request.IsActive == 0)
            {
                var isLinked = await _queryRepository.IsUsageTypeLinkedAsync(request.Id);
                if (isLinked)
                    throw new ExceptionRules(
                        "This master is linked with other records. You cannot inactivate this record.");
            }

            var entity = _mapper.Map<Domain.Entities.UsageType>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "USAGETYPE_UPDATE",
                actionName: request.Id.ToString(),
                details: $"UsageType with Id {request.Id} updated successfully.",
                module: "UsageType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "UsageType updated successfully.",
                Data = result
            };
        }
    }
}
