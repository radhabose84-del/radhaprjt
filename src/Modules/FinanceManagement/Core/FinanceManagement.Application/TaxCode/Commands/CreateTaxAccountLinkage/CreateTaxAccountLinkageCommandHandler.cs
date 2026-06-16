using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Commands.CreateTaxAccountLinkage
{
    public class CreateTaxAccountLinkageCommandHandler : IRequestHandler<CreateTaxAccountLinkageCommand, ApiResponseDTO<int>>
    {
        private readonly ITaxCodeCommandRepository _commandRepository;
        private readonly ITaxCodeQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public CreateTaxAccountLinkageCommandHandler(
            ITaxCodeCommandRepository commandRepository,
            ITaxCodeQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator,
            IMapper mapper)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
            _mapper = mapper;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateTaxAccountLinkageCommand request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            // Initial create is auto-approved (modifications go through the approval workflow).
            var approvedStatusId = await _queryRepository.GetMiscIdAsync("ApprovalStatus", "APPROVED")
                ?? throw new ExceptionRules("ApprovalStatus 'APPROVED' is not configured in MiscMaster.");

            var entity = _mapper.Map<Domain.Entities.TaxAccountLinkage>(request);
            entity.CompanyId = companyId;
            entity.StatusId = approvedStatusId;   // initial create is auto-approved; IsActive=Active via profile

            var newId = await _commandRepository.CreateLinkageAsync(entity);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "TAX_ACCOUNT_LINKAGE_CREATE",
                actionName: newId.ToString(),
                details: $"Tax-account linkage created (TaxCodeId {request.TaxCodeId} -> GlAccountId {request.GlAccountId}) with Id {newId}.",
                module: "TaxAccountLinkage"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Tax-account linkage created successfully.",
                Data = newId
            };
        }
    }
}
