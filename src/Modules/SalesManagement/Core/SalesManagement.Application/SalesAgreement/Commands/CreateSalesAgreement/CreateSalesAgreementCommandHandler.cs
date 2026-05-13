using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly ICompanyLookup _companyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateSalesAgreementCommandHandler> _logger;

        public CreateSalesAgreementCommandHandler(
            ISalesAgreementCommandRepository commandRepository,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IDocumentSequenceLookup documentSequenceLookup,
            IIPAddressService ipAddressService,
            ICompanyLookup companyLookup,
            IUnitLookup unitLookup,
            IMapper mapper,
            IMediator mediator,
            ILogger<CreateSalesAgreementCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _documentSequenceLookup = documentSequenceLookup;
            _ipAddressService = ipAddressService;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateSalesAgreementCommand request, CancellationToken cancellationToken)
        {
            // Pre-check: verify AgentPOAttachment temp file exists on disk before we save the header.
            string? agentPOUploadPath = null;
            if (!string.IsNullOrWhiteSpace(request.AgentPOAttachment))
            {
                agentPOUploadPath = await BuildDocumentUploadPathAsync(MiscEnumEntity.AgentPoDocument);
                var filePath = Path.Combine(agentPOUploadPath, request.AgentPOAttachment);

                if (!File.Exists(filePath))
                {
                    return new ApiResponseDTO<int>
                    {
                        IsSuccess = false,
                        Message = $"Agent PO attachment not found at path: {request.AgentPOAttachment}",
                        Data = 0
                    };
                }
            }

            var entity = _mapper.Map<SalesAgreementHeader>(request);

            // Resolve "Pending" status at runtime (Sales.MiscMaster where MiscTypeId=36/ApprovalStatus).
            var pendingStatus = await _miscMasterQueryRepository.GetMiscMasterByName(
                MiscEnumEntity.SalesAgreementApprovalStatus,
                MiscEnumEntity.SalesAgreementStatusApproved);

            if (pendingStatus == null)
                throw new ExceptionRules("Pending status is not configured in MiscMaster (MiscTypeCode='ApprovalStatus', Code='Pending').");

            entity.StatusId = pendingStatus.Id;

            // Capture the current Unit from the JWT/IP context (cross-module — UserManagement).
            entity.UnitId = _ipAddressService.GetUnitId();

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

            // Post-save: rename AgentPOAttachment from TEMP_{guid}.{ext} → {company}-{unit}-{agreementId}.{ext}
            // and persist the new filename. Failure here is logged but does NOT abort the transaction.
            if (!string.IsNullOrWhiteSpace(request.AgentPOAttachment) && agentPOUploadPath != null)
            {
                try
                {
                    await RenameDocumentAsync(
                        newId,
                        request.AgentPOAttachment,
                        agentPOUploadPath,
                        _commandRepository.UpdateAgentPOAttachmentAsync,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rename Agent PO attachment for Sales Agreement Id {Id}.", newId);
                }
            }

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

        private async Task<string> BuildDocumentUploadPathAsync(string folderName)
        {
            var companies = await _companyLookup.GetAllCompanyAsync();
            var units = await _unitLookup.GetAllUnitAsync();

            var companyName = companies.FirstOrDefault(c => c.CompanyId == (_ipAddressService.GetCompanyId() ?? 0))?.CompanyName ?? string.Empty;
            var unitName = units.FirstOrDefault(u => u.UnitId == (_ipAddressService.GetUnitId() ?? 0))?.UnitName ?? string.Empty;

            return Path.Combine(
                "Resources",
                "SalesAgreement",
                folderName,
                companyName,
                unitName);
        }

        private async Task RenameDocumentAsync(
            int agreementId,
            string originalFileName,
            string uploadPath,
            Func<int, string, CancellationToken, Task<bool>> updateRepo,
            CancellationToken ct)
        {
            var filePath = Path.Combine(uploadPath, originalFileName);
            if (!File.Exists(filePath))
                return;

            var companies = await _companyLookup.GetAllCompanyAsync();
            var units = await _unitLookup.GetAllUnitAsync();

            var companyName = companies.FirstOrDefault(c => c.CompanyId == (_ipAddressService.GetCompanyId() ?? 0))?.CompanyName ?? "comp";
            var unitName = units.FirstOrDefault(u => u.UnitId == (_ipAddressService.GetUnitId() ?? 0))?.UnitName ?? "unit";

            var extension = Path.GetExtension(filePath);
            var newFileName = $"{companyName}-{unitName}-{agreementId}{extension}";
            var newPath = Path.Combine(uploadPath, newFileName);

            File.Move(filePath, newPath, overwrite: true);
            await updateRepo(agreementId, newFileName, ct);
        }
    }
}
