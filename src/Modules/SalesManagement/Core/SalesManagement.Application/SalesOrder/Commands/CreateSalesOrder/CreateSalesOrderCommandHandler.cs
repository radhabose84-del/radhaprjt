using System.Text.Json;
using AutoMapper;
using Contracts.Common;
using Contracts.Commands.Workflow;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using Contracts.Interfaces;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Commands.CreateSalesOrder
{
    public class CreateSalesOrderCommandHandler : IRequestHandler<CreateSalesOrderCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesOrderCommandRepository _commandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;
        private readonly ICompanyLookup _companyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly IDocumentSequenceLookup _documentSequenceLookup;
        private readonly ILogger<CreateSalesOrderCommandHandler> _logger;
        private readonly IOutboxEventPublisher _outboxEventPublisher;

        public CreateSalesOrderCommandHandler(
            ISalesOrderCommandRepository commandRepository,
            IMapper mapper,
            IMediator mediator,
            IIPAddressService ipAddressService,
            ICompanyLookup companyLookup,
            IUnitLookup unitLookup,
            IDocumentSequenceLookup documentSequenceLookup,
            ILogger<CreateSalesOrderCommandHandler> logger,
            IOutboxEventPublisher outboxEventPublisher)
        {
            _commandRepository = commandRepository;
            _mapper = mapper;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
            _documentSequenceLookup = documentSequenceLookup;
            _logger = logger;
            _outboxEventPublisher = outboxEventPublisher;
        }

        public async Task<ApiResponseDTO<int>> Handle(CreateSalesOrderCommand request, CancellationToken cancellationToken)
        {
            var details = request.SalesOrderDetails;

            // Pre-check: verify VisitNotesAttachment exists on disk
            string? visitNotesUploadPath = null;
            if (!string.IsNullOrWhiteSpace(details?.VisitNotesAttachment))
            {
                visitNotesUploadPath = await BuildDocumentUploadPathAsync(MiscEnumEntity.SalesOrderVisitPath);
                var filePath = Path.Combine(visitNotesUploadPath, details.VisitNotesAttachment);

                if (!File.Exists(filePath))
                {
                    return new ApiResponseDTO<int>
                    {
                        IsSuccess = false,
                        Message = $"Visit notes attachment not found at path: {details.VisitNotesAttachment}",
                        Data = 0
                    };
                }
            }

            // Pre-check: verify AgentPOAttachment exists on disk
            string? agentPOUploadPath = null;
            if (!string.IsNullOrWhiteSpace(details?.AgentPOAttachment))
            {
                agentPOUploadPath = await BuildDocumentUploadPathAsync(MiscEnumEntity.AgentPoDocument);
                var filePath = Path.Combine(agentPOUploadPath, details.AgentPOAttachment);

                if (!File.Exists(filePath))
                {
                    return new ApiResponseDTO<int>
                    {
                        IsSuccess = false,
                        Message = $"Agent PO attachment not found at path: {details.AgentPOAttachment}",
                        Data = 0
                    };
                }
            }

            var entity = _mapper.Map<SalesOrderHeader>(details);

            // Generate SalesOrderNo from DocumentSequence
            if (!details!.SalesOrderTypeId.HasValue)
                throw new ExceptionRules("SalesOrderTypeId is required.");

            var sequences = await _documentSequenceLookup.GenerateDocumentNumber(details.SalesOrderTypeId.Value);
            var salesOrderNo = sequences.Count > 0 ? sequences[^1] : null;
            entity.SalesOrderNo = salesOrderNo
                ?? throw new ExceptionRules("No document sequence configured for Sales Order.");
            entity.OrderDate = DateOnly.FromDateTime(DateTime.UtcNow);
            entity.OrderUnitId = _ipAddressService.GetUnitId();

            var newId = await _commandRepository.CreateAsync(entity, details.SalesOrderTypeId.Value);

            // Post-save: rename VisitNotesAttachment to {companyName}-{unitName}-{salesOrderId}.ext
            if (!string.IsNullOrWhiteSpace(details.VisitNotesAttachment) && visitNotesUploadPath != null)
            {
                try
                {
                    await RenameDocumentAsync(
                        newId,
                        details.VisitNotesAttachment,
                        visitNotesUploadPath,
                        _commandRepository.UpdateVisitNotesAttachmentAsync,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rename visit notes attachment for Sales Order Id {SalesOrderId}.", newId);
                }
            }

            // Post-save: rename AgentPOAttachment to {companyName}-{unitName}-{salesOrderId}.ext
            if (!string.IsNullOrWhiteSpace(details.AgentPOAttachment) && agentPOUploadPath != null)
            {
                try
                {
                    await RenameDocumentAsync(
                        newId,
                        details.AgentPOAttachment,
                        agentPOUploadPath,
                        _commandRepository.UpdateAgentPOAttachmentAsync,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rename agent PO attachment for Sales Order Id {SalesOrderId}.", newId);
                }
            }

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALESORDER_CREATE",
                actionName: salesOrderNo,
                details: $"Sales Order '{salesOrderNo}' created successfully with Id {newId}.",
                module: "SalesOrder");
            await _mediator.Publish(auditEvent, cancellationToken);

            // ------------------- Fetch entity for workflow payload -------------------
            var workFlowEntity = await _commandRepository.GetByIdSalesOrderWorkFlowAsync(newId);
            var reverseMap = new CreateSalesOrderReverseDto
            {
                Header = workFlowEntity,
                Lines = null
            };
            string serializedPayload = JsonSerializer.Serialize(reverseMap);

            // ------------------- Schedule Outbox Event (SQL Transactional Outbox) -------------------
            var correlationId = Guid.NewGuid();
            var @event = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.TransactionTypeSalesOrder,
                ModuleTransactionId = newId,
                Payload = serializedPayload
            };

            await _outboxEventPublisher.ScheduleAsync(@event, correlationId, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Order created successfully.",
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
                "SalesOrder",
                folderName,
                companyName,
                unitName);
        }

        private async Task RenameDocumentAsync(
            int salesOrderId,
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
            var newFileName = $"{companyName}-{unitName}-{salesOrderId}{extension}";
            var newPath = Path.Combine(uploadPath, newFileName);

            File.Move(filePath, newPath, overwrite: true);
            await updateRepo(salesOrderId, newFileName, ct);
        }
    }
}
