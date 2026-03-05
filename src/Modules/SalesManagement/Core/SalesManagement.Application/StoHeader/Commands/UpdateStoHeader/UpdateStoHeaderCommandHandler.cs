using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoHeader.Commands.UpdateStoHeader
{
    public class UpdateStoHeaderCommandHandler : IRequestHandler<UpdateStoHeaderCommand, ApiResponseDTO<int>>
    {
        private readonly IStoHeaderCommandRepository _commandRepository;
        private readonly IStoHeaderQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateStoHeaderCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(UpdateStoHeaderCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.StoHeader>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "STO_HEADER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Stock Transfer Order with Id {request.Id} updated successfully.",
                module: "StoHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Stock Transfer Order updated successfully.",
                Data = result
            };
        }
    }
}
