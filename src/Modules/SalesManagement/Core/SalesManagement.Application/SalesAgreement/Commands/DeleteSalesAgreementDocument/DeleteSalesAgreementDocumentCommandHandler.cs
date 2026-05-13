using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesAgreement.Commands.DeleteSalesAgreementDocument
{
    public class DeleteSalesAgreementDocumentCommandHandler : IRequestHandler<DeleteSalesAgreementDocumentCommand, bool>
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;
        private readonly ICompanyLookup _companyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly ILogger<DeleteSalesAgreementDocumentCommandHandler> _logger;

        public DeleteSalesAgreementDocumentCommandHandler(
            IFileUploadService fileUploadService,
            IMediator mediator,
            IIPAddressService ipAddressService,
            ICompanyLookup companyLookup,
            IUnitLookup unitLookup,
            ILogger<DeleteSalesAgreementDocumentCommandHandler> logger)
        {
            _fileUploadService = fileUploadService;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteSalesAgreementDocumentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
                return false;

            // Get company and unit context
            var companyId = _ipAddressService.GetCompanyId() ?? 0;
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            var companies = await _companyLookup.GetAllCompanyAsync();
            var companyName = companies.FirstOrDefault(c => c.CompanyId == companyId)?.CompanyName ?? "Default";

            var units = await _unitLookup.GetAllUnitAsync();
            var unitName = units.FirstOrDefault(u => u.UnitId == unitId)?.UnitName ?? "Default";

            // Reconstruct full file path matching the upload path structure
            var fullPath = Path.Combine("Resources", "SalesAgreement", "AgentPoDocument", companyName, unitName, request.FilePath);

            var result = await _fileUploadService.DeleteFileAsync(fullPath);

            _logger.LogInformation("SalesAgreement document deleted: {FilePath}, Success: {Result}", fullPath, result);

            if (result)
            {
                var auditEvent = new AuditLogsDomainEvent(
                    actionDetail: "Delete",
                    actionCode: "SALESAGREEMENT_DELETE_DOC",
                    actionName: request.FilePath,
                    details: $"Sales Agreement document deleted: {fullPath}",
                    module: "SalesAgreement");
                await _mediator.Publish(auditEvent, cancellationToken);
            }

            return result;
        }
    }
}
