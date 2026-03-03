using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Commands.UpdateSalesOrder
{
    public class UpdateSalesOrderCommandHandler : IRequestHandler<UpdateSalesOrderCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesOrderCommandRepository _commandRepository;
        private readonly ISalesOrderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;
        private readonly ICompanyLookup _companyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly ILogger<UpdateSalesOrderCommandHandler> _logger;

        public UpdateSalesOrderCommandHandler(
            ISalesOrderCommandRepository commandRepository,
            ISalesOrderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator,
            IIPAddressService ipAddressService,
            ICompanyLookup companyLookup,
            IUnitLookup unitLookup,
            ILogger<UpdateSalesOrderCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
            _logger = logger;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateSalesOrderCommand request, CancellationToken cancellationToken)
        {
            // Pre-check: verify VisitNotesAttachment exists on disk
            string? visitNotesUploadPath = null;
            if (!string.IsNullOrWhiteSpace(request.VisitNotesAttachment))
            {
                visitNotesUploadPath = await BuildDocumentUploadPathAsync(MiscEnumEntity.SalesOrderVisitPath);
                var filePath = Path.Combine(visitNotesUploadPath, request.VisitNotesAttachment);

                if (!File.Exists(filePath))
                {
                    return new ApiResponseDTO<int>
                    {
                        IsSuccess = false,
                        Message = $"Visit notes attachment not found at path: {request.VisitNotesAttachment}",
                        Data = 0
                    };
                }
            }

            // Pre-check: verify AgentPOAttachment exists on disk
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

            var entity = _mapper.Map<SalesOrderHeader>(request);

            var result = await _commandRepository.UpdateAsync(entity);

            // Post-save: rename VisitNotesAttachment to {companyName}-{unitName}-{salesOrderId}.ext
            if (!string.IsNullOrWhiteSpace(request.VisitNotesAttachment) && visitNotesUploadPath != null)
            {
                try
                {
                    await RenameDocumentAsync(
                        request.Id,
                        request.VisitNotesAttachment,
                        visitNotesUploadPath,
                        _commandRepository.UpdateVisitNotesAttachmentAsync,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rename visit notes attachment for Sales Order Id {SalesOrderId}.", request.Id);
                }
            }

            // Post-save: rename AgentPOAttachment to {companyName}-{unitName}-{salesOrderId}.ext
            if (!string.IsNullOrWhiteSpace(request.AgentPOAttachment) && agentPOUploadPath != null)
            {
                try
                {
                    await RenameDocumentAsync(
                        request.Id,
                        request.AgentPOAttachment,
                        agentPOUploadPath,
                        _commandRepository.UpdateAgentPOAttachmentAsync,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rename agent PO attachment for Sales Order Id {SalesOrderId}.", request.Id);
                }
            }

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALESORDER_UPDATE",
                actionName: request.Id.ToString(),
                details: $"Sales Order with Id {request.Id} updated successfully.",
                module: "SalesOrder");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Order updated successfully.",
                Data = result
            };
        }

        private async Task<string> BuildDocumentUploadPathAsync(string folderName)
        {
            var companies = await _companyLookup.GetAllCompanyAsync();
            var units = await _unitLookup.GetAllUnitAsync();

            var companyName = companies.FirstOrDefault(c => c.CompanyId == _ipAddressService.GetCompanyId())?.CompanyName ?? string.Empty;
            var unitName = units.FirstOrDefault(u => u.UnitId == _ipAddressService.GetUnitId())?.UnitName ?? string.Empty;

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

            var companyName = companies.FirstOrDefault(c => c.CompanyId == _ipAddressService.GetCompanyId())?.CompanyName ?? "comp";
            var unitName = units.FirstOrDefault(u => u.UnitId == _ipAddressService.GetUnitId())?.UnitName ?? "unit";

            var extension = Path.GetExtension(filePath);
            var newFileName = $"{companyName}-{unitName}-{salesOrderId}{extension}";
            var newPath = Path.Combine(uploadPath, newFileName);

            File.Move(filePath, newPath, overwrite: true);
            await updateRepo(salesOrderId, newFileName, ct);
        }
    }
}
