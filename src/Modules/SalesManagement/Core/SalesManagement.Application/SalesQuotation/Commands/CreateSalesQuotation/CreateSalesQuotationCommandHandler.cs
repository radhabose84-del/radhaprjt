using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesQuotation;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotation
{
    public class CreateSalesQuotationCommandHandler : IRequestHandler<CreateSalesQuotationCommand, int>
    {
        private readonly ISalesQuotationCommandRepository _commandRepository;
        private readonly ISalesQuotationQueryRepository _queryRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateSalesQuotationCommandHandler(
            ISalesQuotationCommandRepository commandRepository,
            ISalesQuotationQueryRepository queryRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<int> Handle(CreateSalesQuotationCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<SalesQuotationHeader>(request);

            // Set default StatusId to 'Pending'
            var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.InvoiceApprovalStatus, MiscEnumEntity.InvoiceStatusPending);
            entity.StatusId = pendingStatus?.Id;

            var newId = await _commandRepository.CreateAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALESQUOTATION_CREATE",
                actionName: newId.ToString(),
                details: $"Sales Quotation created successfully with Id {newId}.",
                module: "SalesQuotation");
            await _mediator.Publish(auditEvent, cancellationToken);

            return newId > 0 ? newId : throw new ExceptionRules("Sales Quotation Creation Failed.");
        }
    }
}
