using System.Text.Json;
using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Command.CreatePurchaseIndent;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.Application.PurchaseIndents.Command.UpdatePurchaseIndent
{
    public class UpdatePurchaseIndentCommandHandler : IRequestHandler<UpdatePurchaseIndentCommand, bool>
    {
        private readonly IPurchaseIndentCommand _purchaseIndentCommand;
        private readonly IMediator _imediator;
        private readonly IMapper _imapper;
        private readonly IPurchaseIndentQuery _purchaseIndentQuery;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IPurchaseUnitOfWork _unitOfWork;
        private readonly ILogger<UpdatePurchaseIndentCommandHandler> _logger;

        public UpdatePurchaseIndentCommandHandler(
            IPurchaseIndentCommand purchaseIndentCommand,
            IMediator imediator,
            IMapper imapper,
            IPurchaseIndentQuery purchaseIndentQuery,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IOutboxEventPublisher outboxEventPublisher,
            IPurchaseUnitOfWork unitOfWork,
            ILogger<UpdatePurchaseIndentCommandHandler> logger)
        {
            _purchaseIndentCommand = purchaseIndentCommand;
            _imediator = imediator;
            _imapper = imapper;
            _purchaseIndentQuery = purchaseIndentQuery;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _outboxEventPublisher = outboxEventPublisher;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdatePurchaseIndentCommand request, CancellationToken cancellationToken)
        {
            var Indent = _imapper.Map<IndentHeader>(request);

            var StatusMisc = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.Status, MiscEnumEntity.Draft);
            var StatusPending = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.Status, MiscEnumEntity.Pending);

            Indent.StatusId = request.IsDraft == 1 ? StatusMisc.Id : StatusPending.Id;

            foreach (var item in Indent.IndentDetails)
            {
                item.StatusId = request.IsDraft == 1 ? StatusMisc.Id : StatusPending.Id;
            }

            var CheckPending = await _purchaseIndentQuery.GetByIdAsync(request.Id);

            _logger.LogInformation("Update Purchase Indent TransactionCreatedEvent Publish check. {IsDraft},{StatusMisc},{CheckPending}",
                request.IsDraft, StatusMisc.Id, CheckPending?.StatusId);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await _purchaseIndentCommand.UpdateAsync(Indent, JsonSerializer.Serialize(request), request.IsApprovalEdit);

                if (result && request.IsDraft == 0)
                {
                    var indentData = await _purchaseIndentQuery.GetByIdAsync(request.Id);
                    var indentReverseMap = _imapper.Map<IndentReverseMapDto>(indentData);
                    var correlationId = Guid.NewGuid();

                    var approvalCommand = new CreateApprovalRequestCommand
                    {
                        CorrelationId = correlationId,
                        ModuleTypeName = MiscEnumEntity.PurchaseIndent,
                        ModuleTransactionId = request.Id,
                        Payload = JsonSerializer.Serialize(indentReverseMap)
                    };

                    await _outboxEventPublisher.ScheduleWithoutSaveAsync(approvalCommand, correlationId, cancellationToken);
                }

                await _unitOfWork.CommitAsync(cancellationToken);

                var evt = new AuditLogsDomainEvent(
                    actionDetail: "Update",
                    actionCode: "Update",
                    actionName: "Update",
                    details: JsonSerializer.Serialize(request),
                    module: "PurchaseIndent"
                );
                await _imediator.Publish(evt, cancellationToken);

                return result == true ? result : throw new ExceptionRules("Indent update failed.");
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
