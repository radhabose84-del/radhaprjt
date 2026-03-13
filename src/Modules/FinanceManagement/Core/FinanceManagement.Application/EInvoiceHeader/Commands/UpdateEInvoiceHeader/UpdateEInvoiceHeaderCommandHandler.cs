using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Commands.UpdateEInvoiceHeader
{
    public class UpdateEInvoiceHeaderCommandHandler : IRequestHandler<UpdateEInvoiceHeaderCommand, ApiResponseDTO<int>>
    {
        private readonly IEInvoiceHeaderCommandRepository _commandRepository;
        private readonly IEInvoiceHeaderQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public UpdateEInvoiceHeaderCommandHandler(
            IEInvoiceHeaderCommandRepository commandRepository,
            IEInvoiceHeaderQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateEInvoiceHeaderCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.EInvoiceHeader>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "EINVOICE_HEADER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"EInvoice Header with Id {request.Id} updated successfully.",
                module: "EInvoiceHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "EInvoice Header updated successfully.",
                Data = result
            };
        }
    }
}
