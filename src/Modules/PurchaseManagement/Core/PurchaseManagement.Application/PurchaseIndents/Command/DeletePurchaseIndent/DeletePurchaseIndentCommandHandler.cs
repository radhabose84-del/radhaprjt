using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using PurchaseManagement.Application.Common.Exceptions;
using PurchaseManagement.Application.Common.Interfaces.ILogService;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Domain.Common;
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
        private readonly ILogServiceCommand _logServiceCommand;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly ILogger<DeletePurchaseIndentCommandHandler> _logger;
        public DeletePurchaseIndentCommandHandler(IPurchaseIndentCommand purchaseIndentCommand, IMediator imediator, IMapper imapper,
            ILogServiceCommand logServiceCommand, IMiscMasterQueryRepository miscMasterQueryRepository, ILogger<DeletePurchaseIndentCommandHandler> logger)
        {
            _purchaseIndentCommand = purchaseIndentCommand;
            _imediator = imediator;
            _imapper = imapper;
            _logServiceCommand = logServiceCommand;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _logger = logger;
        }
        public async Task<bool> Handle(DeletePurchaseIndentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Delete Purchase Indent. Before Delete: {@request}", request);
             var Indent = _imapper.Map<IndentHeader>(request);
            var result = await _purchaseIndentCommand.DeleteAsync(request.Id,Indent);

            // var StatusMisc = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.Status, MiscEnumEntity.Deleted);
            
            _logger.LogInformation("Delete Purchase Indent. After Delete: {@result}", result);
            //  var IndentLog = new IndentLog
            // {
            //     IndentHeaderId = request.Id,
            //     ActionType = "Deleted",
            //     ActionRemarks = "Indent Deleted",
            //     StatusId = StatusMisc.Id
            // };

            //     await _logServiceCommand.CreateAsync(IndentLog);

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