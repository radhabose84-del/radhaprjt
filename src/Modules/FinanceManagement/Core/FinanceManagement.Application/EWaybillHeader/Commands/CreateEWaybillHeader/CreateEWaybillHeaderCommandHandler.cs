using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EWaybillHeader.Commands.CreateEWaybillHeader
{
    public class CreateEWaybillHeaderCommandHandler : IRequestHandler<CreateEWaybillHeaderCommand, ApiResponseDTO<int>>
    {
        private readonly IEWaybillHeaderCommandRepository _commandRepository;
        private readonly IEWaybillHeaderQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateEWaybillHeaderCommandHandler(
            IEWaybillHeaderCommandRepository commandRepository,
            IEWaybillHeaderQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateEWaybillHeaderCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.EWaybillHeader>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "EWAYBILL_HEADER_CREATE",
                actionName: request.EWBNumber ?? string.Empty,
                details: $"EWaybill Header '{request.EWBNumber}' created successfully with Id {newId}.",
                module: "EWaybillHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "EWaybill Header created successfully.",
                Data = newId
            };
        }
    }
}
