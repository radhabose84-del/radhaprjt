using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IUsageType;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.UsageType.Commands.CreateUsageType
{
    public class CreateUsageTypeCommandHandler : IRequestHandler<CreateUsageTypeCommand, ApiResponseDTO<int>>
    {
        private readonly IUsageTypeCommandRepository _commandRepository;
        private readonly IUsageTypeQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateUsageTypeCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(CreateUsageTypeCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.UsageType>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "USAGETYPE_CREATE",
                actionName: request.UsageTypeCode!,
                details: $"UsageType '{request.UsageTypeCode}' created successfully with Id {newId}.",
                module: "UsageType"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "UsageType created successfully.",
                Data = newId
            };
        }
    }
}
