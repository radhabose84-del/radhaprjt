using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoHeader.Commands.CreateStoHeader
{
    public class CreateStoHeaderCommandHandler : IRequestHandler<CreateStoHeaderCommand, ApiResponseDTO<int>>
    {
        private readonly IStoHeaderCommandRepository _commandRepository;
        private readonly IStoHeaderQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateStoHeaderCommandHandler(
            IStoHeaderCommandRepository commandRepository,
            IStoHeaderQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateStoHeaderCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.StoHeader>(request);

            // Generate STO Number
            var stoNumber = await _commandRepository.GenerateNextStoNumberAsync(request.SupplyingPlantId, cancellationToken);
            entity.StoNumber = stoNumber;

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "STO_HEADER_CREATE",
                actionName: stoNumber,
                details: $"Stock Transfer Order '{stoNumber}' created successfully with Id {newId}.",
                module: "StoHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Stock Transfer Order created successfully.",
                Data = newId
            };
        }
    }
}
