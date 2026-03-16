using System.Text.Json;
using AutoMapper;
using Contracts.Commands.Workflow;
using Contracts.Common;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities;
using PurchaseManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace PurchaseManagement.Application.PurchaseIndents.Command.CreatePurchaseIndent
{
    public class CreatePurchaseIndentCommandHandler : IRequestHandler<CreatePurchaseIndentCommand, int>
    {
        private readonly IMapper _imapper;
        private readonly IMediator _mediator;
        private readonly IPurchaseIndentCommand _purchaseIndentCommand;
        private readonly IMiscMasterQueryRepository _miscMasterQueryRepository;
        private readonly IPurchaseIndentQuery _purchaseIndentQuery;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IPurchaseUnitOfWork _unitOfWork;
        private readonly ILogger<CreatePurchaseIndentCommandHandler> _logger;

        public CreatePurchaseIndentCommandHandler(
            IPurchaseIndentCommand purchaseIndentCommand,
            IMapper imapper,
            IMediator mediator,
            IMiscMasterQueryRepository miscMasterQueryRepository,
            IPurchaseIndentQuery purchaseIndentQuery,
            IOutboxEventPublisher outboxEventPublisher,
            IPurchaseUnitOfWork unitOfWork,
            ILogger<CreatePurchaseIndentCommandHandler> logger)
        {
            _purchaseIndentCommand = purchaseIndentCommand;
            _imapper = imapper;
            _mediator = mediator;
            _miscMasterQueryRepository = miscMasterQueryRepository;
            _purchaseIndentQuery = purchaseIndentQuery;
            _outboxEventPublisher = outboxEventPublisher;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<int> Handle(CreatePurchaseIndentCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Create Purchase Indent. Before Creation: {@request}", request);

            var Indent = _imapper.Map<IndentHeader>(request);

            var IndentNumber = await _purchaseIndentQuery.GeneratePurchaseIndentNumberAsync(request.UnitId);
            Indent.IndentNumber = IndentNumber;

            var StatusMisc = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.Status, MiscEnumEntity.Draft);
            var StatusPending = await _miscMasterQueryRepository.GetMiscMasterByName(MiscEnumEntity.Status, MiscEnumEntity.Pending);

            Indent.StatusId = request.IsDraft == 1 ? StatusMisc.Id : StatusPending.Id;

            foreach (var item in Indent.IndentDetails)
            {
                item.StatusId = request.IsDraft == 1 ? StatusMisc.Id : StatusPending.Id;
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await _purchaseIndentCommand.CreateAsync(Indent);

                _logger.LogInformation("Create Purchase Indent. After Creation: {@result}", result);

                if (result.Id > 0 && request.IsDraft == 0)
                {
                    var indentReverseMap = _imapper.Map<IndentReverseMapDto>(result);
                    var correlationId = Guid.NewGuid();

                    var approvalCommand = new CreateApprovalRequestCommand
                    {
                        CorrelationId = correlationId,
                        ModuleTypeName = MiscEnumEntity.PurchaseIndent,
                        ModuleTransactionId = result.Id,
                        Payload = JsonSerializer.Serialize(indentReverseMap)
                    };

                    await _outboxEventPublisher.ScheduleWithoutSaveAsync(approvalCommand, correlationId, cancellationToken);
                }

                await _unitOfWork.CommitAsync(cancellationToken);

                var evt = new AuditLogsDomainEvent(
                    actionDetail: "Create",
                    actionCode: "Create",
                    actionName: "Create",
                    details: JsonSerializer.Serialize(request),
                    module: "PurchaseIndent"
                );
                await _mediator.Publish(evt, cancellationToken);

                return result.Id > 0 ? result.Id : throw new ExceptionRules("Indent Creation Failed.");
            }
            catch
            {
                await _unitOfWork.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}
