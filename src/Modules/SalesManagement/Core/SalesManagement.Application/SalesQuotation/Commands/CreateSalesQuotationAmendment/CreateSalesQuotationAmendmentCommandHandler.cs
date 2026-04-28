using System.Text.Json;
using Contracts.Commands.Workflow;
using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IOutbox;
using SalesManagement.Application.Common.Interfaces.ISalesQuotationAmendment;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Entities;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.SalesQuotation.Commands.CreateSalesQuotationAmendment
{
    public class CreateSalesQuotationAmendmentCommandHandler
        : IRequestHandler<CreateSalesQuotationAmendmentCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesQuotationAmendmentCommandRepository _commandRepository;
        private readonly IOutboxEventPublisher _outboxEventPublisher;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public CreateSalesQuotationAmendmentCommandHandler(
            ISalesQuotationAmendmentCommandRepository commandRepository,
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
            CreateSalesQuotationAmendmentCommand request, CancellationToken cancellationToken)
        {
            // Fetch the Quotation entity (with details) to snapshot Old* values and get RevisionNumber
            var sqHeader = await _commandRepository.GetSalesQuotationEntityAsync(request.SalesQuotationHeaderId);
            if (sqHeader == null)
                throw new ExceptionRules("Sales quotation not found.");

            // Derive AmendmentNo: {QuotationNo}/AMD/{RevisionNumber}
            var revisionNumber = sqHeader.RevisionNumber + 1;
            var amendmentNo = $"{sqHeader.QuotationNo}/AMD/{revisionNumber}";

            var header = new SalesQuotationAmendmentHeader
            {
                SalesQuotationHeaderId = request.SalesQuotationHeaderId,
                UnitId = _ipAddressService.GetUnitId() ?? 0,
                AmendmentNo = amendmentNo,
                RevisionNumber = revisionNumber,
                AmendmentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                Reason = request.Reason,
                FreightCharges = request.FreightCharges,
                OtherCharges = request.OtherCharges,
                TotalBasicAmount = request.TotalBasicAmount,
                TotalDiscount = request.TotalDiscount,
                NetTaxableAmount = request.NetTaxableAmount,
                TotalTax = request.TotalTax,
                GrandTotal = request.GrandTotal,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            // Build detail rows from the command — snapshot Old* values from Quotation detail lines
            // ChangeType is auto-derived: any New* value → "Modified", all null → "Removed"
            var details = new List<SalesQuotationAmendmentDetail>();
            foreach (var dto in request.AmendmentDetails ?? [])
            {
                var sqDetail = sqHeader.SalesQuotationDetails?
                    .FirstOrDefault(d => d.Id == dto.SalesQuotationDetailId);
                if (sqDetail == null)
                    throw new ExceptionRules($"Sales quotation detail Id {dto.SalesQuotationDetailId} not found.");

                var hasNewValues = dto.NewItemId.HasValue
                    || dto.NewQuantity.HasValue
                    || dto.NewExMillRate.HasValue
                    || dto.NewDiscount.HasValue
                    || dto.NewHSNId.HasValue
                    || dto.NewTaxPercentage.HasValue;

                var changeType = hasNewValues ? "Modified" : "Removed";

                details.Add(new SalesQuotationAmendmentDetail
                {
                    ChangeType = changeType,
                    SalesQuotationDetailId = dto.SalesQuotationDetailId,
                    OldItemId = sqDetail.ItemId,
                    OldQuantity = sqDetail.Quantity,
                    OldExMillRate = sqDetail.ExMillRate,
                    OldDiscount = sqDetail.Discount,
                    OldHSNId = sqDetail.HSNId,
                    OldTaxPercentage = sqDetail.TaxPercentage,
                    NewItemId = dto.NewItemId,
                    NewQuantity = dto.NewQuantity,
                    NewExMillRate = dto.NewExMillRate,
                    NewDiscount = dto.NewDiscount,
                    NewHSNId = dto.NewHSNId,
                    NewTaxPercentage = dto.NewTaxPercentage,
                    NetRate = dto.NetRate,
                    TotalAmount = dto.TotalAmount,
                    TaxAmount = dto.TaxAmount
                });
            }

            var newId = await _commandRepository.CreateAsync(header, details);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALESQUOTATIONAMENDMENT_CREATE",
                actionName: amendmentNo,
                details: $"Sales Quotation Amendment '{amendmentNo}' created for Quotation Id {request.SalesQuotationHeaderId} with Id {newId}.",
                module: "SalesQuotationAmendment"), cancellationToken);

            // Fetch entity for workflow payload
            var workFlowEntity = await _commandRepository.GetByIdAmendmentWorkFlowAsync(newId);
            var reverseMap = new CreateSalesQuotationAmendmentReverseDto
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
                ModuleTypeName = MiscEnumEntity.TransactionTypeSalesQuotationAmendment,
                ModuleTransactionId = newId,
                Payload = serializedPayload
            };
            await _outboxEventPublisher.ScheduleAsync(@event, correlationId, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Quotation Amendment created successfully.",
                Data = newId
            };
        }
    }
}
