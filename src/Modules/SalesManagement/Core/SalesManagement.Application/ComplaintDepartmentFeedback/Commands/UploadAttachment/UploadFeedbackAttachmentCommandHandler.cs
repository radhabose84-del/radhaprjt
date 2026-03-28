using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.ComplaintDepartmentFeedback.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ComplaintDepartmentFeedback.Commands.UploadAttachment
{
    public class UploadFeedbackAttachmentCommandHandler : IRequestHandler<UploadFeedbackAttachmentCommand, FeedbackAttachmentUploadDto>
    {
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;
        private readonly ICompanyLookup _companyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly ILogger<UploadFeedbackAttachmentCommandHandler> _logger;

        public UploadFeedbackAttachmentCommandHandler(
            IMediator mediator,
            IIPAddressService ipAddressService,
            ICompanyLookup companyLookup,
            IUnitLookup unitLookup,
            ILogger<UploadFeedbackAttachmentCommandHandler> logger)
        {
            _mediator = mediator;
            _ipAddressService = ipAddressService;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
            _logger = logger;
        }

        public async Task<FeedbackAttachmentUploadDto> Handle(UploadFeedbackAttachmentCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
                throw new ExceptionRules("File is required.");

            var companyId = _ipAddressService.GetCompanyId() ?? 0;
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            var companies = await _companyLookup.GetAllCompanyAsync();
            var companyName = companies.FirstOrDefault(c => c.CompanyId == companyId)?.CompanyName ?? "Default";

            var units = await _unitLookup.GetAllUnitAsync();
            var unitName = units.FirstOrDefault(u => u.UnitId == unitId)?.UnitName ?? "Default";

            var uploadPath = Path.Combine("Resources", "ComplaintFeedback", "Attachments", companyName, unitName);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var extension = Path.GetExtension(request.File.FileName);
            var fileName = $"CFB_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream, cancellationToken);
            }

            _logger.LogInformation("Complaint feedback attachment uploaded: {FileName} at {FilePath}", fileName, filePath);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Upload",
                actionCode: "COMPLAINT_FEEDBACK_UPLOAD",
                actionName: fileName,
                details: $"Complaint feedback attachment uploaded: {filePath}",
                module: "ComplaintDepartmentFeedback");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new FeedbackAttachmentUploadDto
            {
                FileName = fileName,
                FilePath = filePath,
                FileType = request.File.ContentType,
                FileSize = request.File.Length
            };
        }
    }
}
