using System.Text.Json;
using AutoMapper;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.Application.PurchaseIndents.Command.DeletePurchaseIndent
{
    public class DeletePurchaseIndentCommandHandler : IRequestHandler<DeletePurchaseIndentCommand, bool>
    {
        private readonly IPurchaseIndentCommand _purchaseIndentCommand;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        private readonly ILogger<DeletePurchaseIndentCommandHandler> _logger;

        public DeletePurchaseIndentCommandHandler(IPurchaseIndentCommand purchaseIndentCommand, IMediator imediator, IMapper imapper,
            ILogger<DeletePurchaseIndentCommandHandler> logger)
        {
            _purchaseIndentCommand = purchaseIndentCommand;
            _imediator = imediator;
            _imapper = imapper;
            _logger = logger;
        }

        public async Task<bool> Handle(DeletePurchaseIndentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Delete Purchase Indent. Before Delete: {@request}", request);
            var Indent = _imapper.Map<IndentHeader>(request);
            var result = await _purchaseIndentCommand.DeleteAsync(request.Id, Indent);

            _logger.LogInformation("Delete Purchase Indent. After Delete: {@result}", result);

            var evt = new AuditLogsDomainEvent(
                actionDetail: "Delete",
                actionCode: "Delete",
                actionName: "Delete",
                details: JsonSerializer.Serialize(request),
                module: "PurchaseIndent"
            );
            await _imediator.Publish(evt, cancellationToken);

            return result == true ? result : throw new ExceptionRules("Indent deletion failed.");
        }
    }
}
