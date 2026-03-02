using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Commands.DeleteSalesOrderDocument
{
    public class DeleteSalesOrderDocumentCommandHandler : IRequestHandler<DeleteSalesOrderDocumentCommand, bool>
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly IMediator _mediator;

        public DeleteSalesOrderDocumentCommandHandler(
            IFileUploadService fileUploadService,
            IMediator mediator)
        {
            _fileUploadService = fileUploadService;
            _mediator = mediator;
        }

        public async Task<bool> Handle(DeleteSalesOrderDocumentCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.FilePath))
                throw new ExceptionRules("File path is required.");

            var result = await _fileUploadService.DeleteFileAsync(request.FilePath);

            if (result)
            {
                var auditEvent = new AuditLogsDomainEvent(
                    actionDetail: "Delete",
                    actionCode: "SALESORDER_DELETE_DOC",
                    actionName: request.FilePath,
                    details: $"Sales Order document deleted: {request.FilePath}",
                    module: "SalesOrder");
                await _mediator.Publish(auditEvent, cancellationToken);
            }

            return result;
        }
    }
}
