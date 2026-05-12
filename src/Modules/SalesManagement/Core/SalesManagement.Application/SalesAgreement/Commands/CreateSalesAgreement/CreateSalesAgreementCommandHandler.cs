using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesAgreement.Commands.CreateSalesAgreement
{
    public class CreateSalesAgreementCommandHandler : IRequestHandler<CreateSalesAgreementCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesAgreementCommandRepository _commandRepository;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public CreateSalesAgreementCommandHandler(
            ISalesAgreementCommandRepository commandRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            IMapper mapper,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateSalesAgreementCommand request, CancellationToken cancellationToken)
        {
            var entity = _mapper.Map<SalesAgreementHeader>(request);

            // Resolve "Pending" status at runtime (Sales.MiscMaster where MiscTypeId=36/ApprovalStatus).
            var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.SalesAgreementApprovalStatus,
                MiscEnumEntity.SalesAgreementStatusApproved);

            if (pendingStatus == null)
                throw new ExceptionRules("Pending status is not configured in MiscMaster (MiscTypeCode='ApprovalStatus', Code='Pending').");

            entity.StatusId = pendingStatus.Id;

            // Resolve TransactionTypeId for "Sales Agreement" → Finance.TransactionTypeMaster.
            var unitId = _ipAddressService.GetUnitId() ?? 0;
            var typeId = await _documentSequenceLookup.GetTransactionTypeIdAsync(
                MiscEnumEntity.TransactionTypeSalesAgreement,
                MiscEnumEntity.ModuleSales,
                unitId);

            if (!typeId.HasValue)
                throw new ExceptionRules("Transaction Type 'Sales Agreement' not found. Please configure it in Transaction Type Master.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(typeId.Value);
            var agreementNo = sequences.Count > 0 ? sequences[^1] : null;
            entity.AgreementNo = agreementNo
                ?? throw new ExceptionRules("No document sequence configured for Sales Agreement.");

            var newId = await _commandRepository.CreateAsync(entity, typeId.Value);

            if (newId <= 0)
                throw new ExceptionRules("Sales Agreement creation failed.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALESAGREEMENT_CREATE",
                actionName: agreementNo,
                details: $"Sales Agreement '{agreementNo}' created successfully with Id {newId}.",
                module: "SalesAgreement");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Agreement created successfully.",
                Data = newId
            };
        }
    }
}
