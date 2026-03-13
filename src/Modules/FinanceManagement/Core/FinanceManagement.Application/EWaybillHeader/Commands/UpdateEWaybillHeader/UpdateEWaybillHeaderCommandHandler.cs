using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EWaybillHeader.Commands.UpdateEWaybillHeader
{
    public class UpdateEWaybillHeaderCommandHandler : IRequestHandler<UpdateEWaybillHeaderCommand, ApiResponseDTO<int>>
    {
        private readonly IEWaybillHeaderCommandRepository _commandRepository;
        private readonly IEWaybillHeaderQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateEWaybillHeaderCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(UpdateEWaybillHeaderCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.EWaybillHeader>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "EWAYBILL_HEADER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"EWaybill Header with Id {request.Id} updated successfully.",
                module: "EWaybillHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "EWaybill Header updated successfully.",
                Data = result
            };
        }
    }
}
