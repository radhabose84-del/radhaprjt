using AutoMapper;
using Contracts.Common;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.EInvoiceHeader.Commands.CreateEInvoiceHeader
{
    public class CreateEInvoiceHeaderCommandHandler : IRequestHandler<CreateEInvoiceHeaderCommand, ApiResponseDTO<int>>
    {
        private readonly IEInvoiceHeaderCommandRepository _commandRepository;
        private readonly IEInvoiceHeaderQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateEInvoiceHeaderCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(CreateEInvoiceHeaderCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.EInvoiceHeader>(request);

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "EINVOICE_HEADER_CREATE",
                actionName: request.InvoiceNo ?? string.Empty,
                details: $"EInvoice Header '{request.InvoiceNo}' created successfully with Id {newId}.",
                module: "EInvoiceHeader"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "EInvoice Header created successfully.",
                Data = newId
            };
        }
    }
}
