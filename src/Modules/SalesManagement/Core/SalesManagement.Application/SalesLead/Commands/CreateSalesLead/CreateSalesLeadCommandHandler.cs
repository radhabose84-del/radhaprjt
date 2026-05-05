using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.SalesLead.Commands.CreateSalesLead
{
    public class CreateSalesLeadCommandHandler : IRequestHandler<CreateSalesLeadCommand, ApiResponseDTO<CreateSalesLeadResponseDto>>
    {
        private readonly ISalesLeadCommandRepository _commandRepository;
        private readonly ISalesLeadQueryRepository _queryRepository;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMarketingOfficerAccessFilter _accessFilter;

        public CreateSalesLeadCommandHandler(
            ISalesLeadCommandRepository commandRepository,
            ISalesLeadQueryRepository queryRepository,
            IMediator mediator,
            IMapper mapper,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            IMarketingOfficerAccessFilter accessFilter)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mediator = mediator;
            _mapper = mapper;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
            _accessFilter = accessFilter;
        }

        public async Task<ApiResponseDTO<CreateSalesLeadResponseDto>> Handle(CreateSalesLeadCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<Domain.Entities.SalesLead>(request);

            // Build contact entity (if needed) — NOT persisted yet; passed to repository for transactional creation
            Domain.Entities.SalesContact? newContact = null;
            if (!request.ContactId.HasValue && !string.IsNullOrWhiteSpace(request.ContactName))
            {
                var primaryContactTypeId = await _queryRepository.GetPrimaryContactTypeIdAsync();
                newContact = new Domain.Entities.SalesContact
                {
                    ContactName = request.ContactName,
                    MobileNumber = request.MobileNumber,
                    Email = request.EmailId,
                    PartyId = request.PartyId,
                    ContactTypeId = primaryContactTypeId,
                    IsActive = Status.Active,
                    IsDeleted = IsDelete.NotDeleted
                };
            }

            // Generate LeadNo from DocumentSequence
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeSalesLead,
                MiscEnumEntity.ModuleSales,
                unitId);

            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'Sales Lead' not found. Please configure it in Transaction Type Master.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var leadNo = sequences.Count > 0 ? sequences[^1] : null;
            entity.LeadNo = leadNo
                ?? throw new ExceptionRules("No document sequence configured for Sales Lead.");

            // Contact + Lead + DocNo increment all happen inside a single transaction
            var newId = await _commandRepository.CreateAsync(entity, typeId.Value, newContact);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALES_LEAD_CREATE",
                actionName: leadNo,
                details: $"Sales Lead '{leadNo}' created successfully with Id {newId}.",
                module: "SalesLead"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<CreateSalesLeadResponseDto>
            {
                IsSuccess = true,
                Message = $"Sales Lead {leadNo} created successfully.",
                Data = new CreateSalesLeadResponseDto
                {
                    Id = newId,
                    LeadNo = leadNo
                }
            };
        }
    }
}
