using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Dto;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.Complaint.Commands.UploadAttachment
{
    public class UploadComplaintAttachmentCommandHandler : IRequestHandler<UploadComplaintAttachmentCommand, ComplaintAttachmentDto>
    {
        private readonly IComplaintCommandRepository _commandRepository;
        private readonly IMediator _mediator;
        private readonly IIPAddressService _ipAddressService;
        private readonly ICompanyLookup _companyLookup;
        private readonly IUnitLookup _unitLookup;
        private readonly ILogger<UploadComplaintAttachmentCommandHandler> _logger;

        public UploadComplaintAttachmentCommandHandler(
            IComplaintCommandRepository commandRepository,
            IMediator mediator,
            IIPAddressService ipAddressService,
            ICompanyLookup companyLookup,
            IUnitLookup unitLookup,
            ILogger<UploadComplaintAttachmentCommandHandler> logger)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
            _ipAddressService = ipAddressService;
            _companyLookup = companyLookup;
            _unitLookup = unitLookup;
            _logger = logger;
        }

        public async Task<ComplaintAttachmentDto> Handle(UploadComplaintAttachmentCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
                throw new ExceptionRules("File is required.");

            var companyId = _ipAddressService.GetCompanyId() ?? 0;
            var unitId = _ipAddressService.GetUnitId() ?? 0;

            var companies = await _companyLookup.GetAllCompanyAsync();
            var companyName = companies.FirstOrDefault(c => c.CompanyId == companyId)?.CompanyName ?? "Default";

            var units = await _unitLookup.GetAllUnitAsync();
            var unitName = units.FirstOrDefault(u => u.UnitId == unitId)?.UnitName ?? "Default";

            var uploadPath = Path.Combine("Resources", "Complaint", "Attachments", companyName, unitName);

            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            var extension = Path.GetExtension(request.File.FileName);
            var fileName = $"CA_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream, cancellationToken);
            }

            var attachment = new ComplaintAttachment
            {
                ComplaintHeaderId = request.ComplaintHeaderId,
                FileName = fileName,
                FilePath = filePath,
                FileType = request.File.ContentType,
                FileSize = request.File.Length,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var newId = await _commandRepository.AddAttachmentAsync(attachment);

            _logger.LogInformation("Complaint attachment uploaded: {FileName} for ComplaintHeaderId {Id}", fileName, request.ComplaintHeaderId);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Upload",
                actionCode: "COMPLAINT_ATTACHMENT_UPLOAD",
                actionName: fileName,
                details: $"Complaint attachment uploaded for ComplaintHeaderId {request.ComplaintHeaderId}: {filePath}",
                module: "Complaint");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ComplaintAttachmentDto
            {
                Id = newId,
                ComplaintHeaderId = request.ComplaintHeaderId,
                FileName = fileName,
                FilePath = filePath,
                FileType = request.File.ContentType,
                FileSize = request.File.Length
            };
        }
    }
}
