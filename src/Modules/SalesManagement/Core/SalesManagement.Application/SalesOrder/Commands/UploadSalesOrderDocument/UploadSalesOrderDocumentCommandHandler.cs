using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Commands.UploadSalesOrderDocument
{
    public class UploadSalesOrderDocumentCommandHandler : IRequestHandler<UploadSalesOrderDocumentCommand, SalesOrderDocumentDto>
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IMediator _mediator;

        public UploadSalesOrderDocumentCommandHandler(
            IFileUploadService fileUploadService,
            IMediator mediator)
        {
            _fileUploadService = fileUploadService;
            _mediator = mediator;
        }

        public async Task<SalesOrderDocumentDto> Handle(UploadSalesOrderDocumentCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
                throw new ExceptionRules("File is required.");

            var uploadPath = $"Resources/SalesOrder/{request.DocumentType}";
            var (isSuccess, filePath, base64) = await _fileUploadService.UploadFileAsync(request.File, uploadPath);

            if (!isSuccess)
                throw new ExceptionRules("File upload failed.");

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Upload",
                actionCode: "SALESORDER_UPLOAD",
                actionName: request.DocumentType ?? "Document",
                details: $"Sales Order document uploaded: {filePath}",
                module: "SalesOrder");
            await _mediator.Publish(auditEvent, cancellationToken);

            return new SalesOrderDocumentDto
            {
                ImagePath = filePath,
                DocumentBase64 = base64
            };
        }
    }
}
