using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using Contracts.Interfaces;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Commands.DeleteSalesOrderImage
{
    public class DeleteSalesOrderImageCommandHandler : IRequestHandler<DeleteSalesOrderImageCommand, bool>
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;
        private readonly ICompanyLookup _companyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly ILogger<DeleteSalesOrderImageCommandHandler> _logger;

        public DeleteSalesOrderImageCommandHandler(
            IFileUploadService fileUploadService,
            IMediator mediator,
            IIPAddressService ipAddressService,
            ICompanyLookup companyLookup,
            IUnitLookup unitLookup,
            ILogger<DeleteSalesOrderImageCommandHandler> logger)
        {
            _fileUploadService = fileUploadService;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteSalesOrderImageCommand request, CancellationToken cancellationToken)
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
            var fullPath = Path.Combine("Resources", "SalesOrder", "SalesOrderVisitPath", companyName, unitName, request.FilePath);

            var result = await _fileUploadService.DeleteFileAsync(fullPath);

            _logger.LogInformation("SalesOrder image deleted: {FilePath}, Success: {Result}", fullPath, result);

            if (result)
            {
                var auditEvent = new AuditLogsDomainEvent(
                    actionDetail: "Delete",
                    actionCode: "SALESORDER_IMAGE_DELETE",
                    actionName: request.FilePath,
                    details: $"Sales Order image deleted: {fullPath}",
                    module: "SalesOrder");
                await _mediator.Publish(auditEvent, cancellationToken);
            }

            return result;
        }
    }
}
