using System.Text.Json;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.Common.Interfaces.ISalesOrderAmendment;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.SalesOrder.Commands.CreateSalesOrderAmendment
{
    public class CreateSalesOrderAmendmentCommandHandler
        : IRequestHandler<CreateSalesOrderAmendmentCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesOrderAmendmentCommandRepository _commandRepository;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public CreateSalesOrderAmendmentCommandHandler(
            ISalesOrderAmendmentCommandRepository commandRepository,
            IOutboxEventPublisher outboxEventPublisher,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _outboxEventPublisher = outboxEventPublisher;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(
            CreateSalesOrderAmendmentCommand request, CancellationToken cancellationToken)
        {
            // Fetch the SO entity (with details) to snapshot Old* values and get RevisionNumber
            var soHeader = await _commandRepository.GetSalesOrderEntityAsync(request.SalesOrderHeaderId);
            if (soHeader == null)
                throw new ExceptionRules("Sales order not found.");

            // Derive AmendmentNo: {SalesOrderNo}/AMD/{RevisionNumber}
            var revisionNumber = soHeader.RevisionNumber + 1;
            var amendmentNo = $"{soHeader.SalesOrderNo}/AMD/{revisionNumber}";

            var header = new SalesOrderAmendmentHeader
            {
                SalesOrderHeaderId = request.SalesOrderHeaderId,
                UnitId = _ipAddressService.GetUnitId() ?? 0,
                AmendmentNo = amendmentNo,
                RevisionNumber = revisionNumber,
                AmendmentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Reason = request.Reason,
                TotalBags = request.TotalBags,
                TotalWeightKgs = request.TotalWeightKgs,
                TotalDiscountPerKg = request.TotalDiscountPerKg,
                ItemValue = request.ItemValue,
                TotalFreight = request.TotalFreight,
                TaxableAmount = request.TaxableAmount,
                GSTPercentage = request.GSTPercentage,
                TotalGST = request.TotalGST,
                TotalWithGST = request.TotalWithGST,
                TCSPercentage = request.TCSPercentage,
                TotalTCS = request.TotalTCS,
                FinalAmount = request.FinalAmount,
                AgentCommissionId = request.AgentCommissionId,
                AgentCommissionSlabId = request.AgentCommissionSlabId,
                AgentPaymentTermsId = request.AgentPaymentTermsId,
                CommissionRate = request.CommissionRate,
                CommissionValue = request.CommissionValue,
                MdDiscountValue = request.MdDiscountValue,
                TotalDiscountValue = request.TotalDiscountValue,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            // Build detail rows from the command — snapshot Old* values from SO detail lines
            // ChangeType is auto-derived: any New* value → "Modified", all null → "Removed"
            var details = new List<SalesOrderAmendmentDetail>();
            foreach (var dto in request.AmendmentDetails ?? [])
            {
                var soDetail = soHeader.SalesOrderDetails?
                    .FirstOrDefault(d => d.Id == dto.SalesOrderDetailId);
                if (soDetail == null)
                    throw new ExceptionRules($"Sales order detail Id {dto.SalesOrderDetailId} not found.");

                var hasNewValues = dto.NewQtyInBags.HasValue
                    || dto.NewExMillRate.HasValue
                    || dto.NewExpectedDeliveryDate.HasValue;

                var changeType = hasNewValues ? "Modified" : "Removed";

                details.Add(new SalesOrderAmendmentDetail
                {
                    ChangeType = changeType,
                    SalesOrderDetailId = dto.SalesOrderDetailId,
                    OldItemId = soDetail.VariantId ?? 0,
                    OldQtyInBags = soDetail.QtyInBags,
                    OldExMillRate = soDetail.ExMillRate,
                    OldExpectedDeliveryDate = soDetail.ExpectedDeliveryDate,
                    NewQtyInBags = dto.NewQtyInBags,
                    NewExMillRate = dto.NewExMillRate,
                    NewExpectedDeliveryDate = dto.NewExpectedDeliveryDate,
                    TaxableAmount = dto.TaxableAmount,
                    TaxAmount = dto.TaxAmount,
                    TCSAmount = dto.TCSAmount,
                    NetAmount = dto.NetAmount,
                    NetRatePerKg = dto.NetRatePerKg,
                    PendingQty = dto.PendingQty,
                    AgentCommissionPercentage = dto.AgentCommissionPercentage
                });
            }

            // Build discount snapshot rows from request
            var discounts = new List<SalesOrderAmendmentDiscount>();
            foreach (var d in request.Discounts ?? [])
            {
                discounts.Add(new SalesOrderAmendmentDiscount
                {
                    SalesOrderDiscountId = d.SalesOrderDiscountId,
                    DiscountMasterId = d.DiscountMasterId,
                    SlabTypeId = d.SlabTypeId,
                    PaymentTermId = d.PaymentTermId,
                    DiscountSlabId = d.DiscountSlabId,
                    DiscountRate = d.DiscountRate,
                    TotalDiscountValue = d.TotalDiscountValue
                });
            }

            var newId = await _commandRepository.CreateAsync(header, details, discounts);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALESORDERAMENDMENT_CREATE",
                actionName: amendmentNo,
                details: $"Sales Order Amendment '{amendmentNo}' created for SO Id {request.SalesOrderHeaderId} with Id {newId}.",
                module: "SalesOrderAmendment"), cancellationToken);

            // Fetch entity for workflow payload
            var workFlowEntity = await _commandRepository.GetByIdAmendmentWorkFlowAsync(newId);
            var reverseMap = new CreateSalesOrderAmendmentReverseDto
            {
                Header = workFlowEntity,
                Lines = null
            };
            string serializedPayload = JsonSerializer.Serialize(reverseMap);

            // Schedule Outbox Event (SQL Transactional Outbox)
            var correlationId = Guid.NewGuid();
            var @event = new CreateApprovalRequestCommand
            {
                CorrelationId = correlationId,
                ModuleTypeName = MiscEnumEntity.TransactionTypeSalesOrderAmendment,
                ModuleTransactionId = newId,
                Payload = serializedPayload
            };
            await _outboxEventPublisher.ScheduleAsync(@event, correlationId, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Order Amendment created successfully.",
                Data = newId
            };
        }
    }
}
