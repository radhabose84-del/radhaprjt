using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Commands.UploadSalesOrderDocument
{
    public class UploadSalesOrderDocumentCommandHandler : IRequestHandler<UploadSalesOrderDocumentCommand, SalesOrderDocumentDto>
    {
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;
        private readonly ICompanyLookup _companyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly ILogger<UploadSalesOrderDocumentCommandHandler> _logger;

        public UploadSalesOrderDocumentCommandHandler(
            IMediator mediator,
            IIPAddressService ipAddressService,
            ICompanyLookup companyLookup,
            IUnitLookup unitLookup,
            ILogger<UploadSalesOrderDocumentCommandHandler> logger)
        {
            _mediator = mediator;
            _ipAddressService = ipAddressService;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
            _logger = logger;
        }

        public async Task<SalesOrderDocumentDto> Handle(UploadSalesOrderDocumentCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
                throw new ExceptionRules("File is required.");

            // Get company and unit context
            var companyId = _ipAddressService.GetCompanyId();
            var unitId = _ipAddressService.GetUnitId();

            var companies = await _companyLookup.GetAllCompanyAsync();
            var companyName = companies.FirstOrDefault(c => c.CompanyId == companyId)?.CompanyName ?? "Default";

            var units = await _unitLookup.GetAllUnitAsync();
            var unitName = units.FirstOrDefault(u => u.UnitId == unitId)?.UnitName ?? "Default";

            // Construct upload path
            var uploadPath = Path.Combine("Resources", "SalesOrder","AgentPoDocument", companyName, unitName);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            // Generate unique filename
            var extension = Path.GetExtension(request.File.FileName);
            var fileName = $"TEMP_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream, cancellationToken);
            }

            // Convert to Base64
            string? imageBase64 = null;
            if (File.Exists(filePath))
            {
                var fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
                imageBase64 = Convert.ToBase64String(fileBytes);
            }

            _logger.LogInformation("SalesOrder document uploaded: {FileName} at {FilePath}", fileName, filePath);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Upload",
                actionCode: "SALESORDER_UPLOAD",
                actionName: fileName,
                details: $"Sales Order document uploaded: {filePath}",
                module: "SalesOrder");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new SalesOrderDocumentDto
            {
                ImageName = fileName,
                ImageBase64 = imageBase64
            };
        }
    }
}
