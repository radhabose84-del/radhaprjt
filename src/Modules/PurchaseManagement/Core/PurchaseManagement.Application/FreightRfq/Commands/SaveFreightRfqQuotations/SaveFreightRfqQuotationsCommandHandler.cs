using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Domain.Entities.FreightRfq;
using PurchaseManagement.Domain.Events;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.Application.FreightRfq.Commands.SaveFreightRfqQuotations
{
    public class SaveFreightRfqQuotationsCommandHandler : IRequestHandler<SaveFreightRfqQuotationsCommand, ApiResponseDTO<int>>
    {
        private readonly IFreightRfqCommandRepository _commandRepository;
        private readonly IMediator _mediator;

        public SaveFreightRfqQuotationsCommandHandler(
            IFreightRfqCommandRepository commandRepository,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(SaveFreightRfqQuotationsCommand request, CancellationToken cancellationToken)
        {
            var rows = request.Quotations.Select(q => new FreightRfqQuotation
            {
                TransporterId = q.TransporterId,
                RateBasisId = q.RateBasisId,
                QuotedRate = q.QuotedRate,
                NoOfVehicles = q.NoOfVehicles,
                Remarks = q.Remarks,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            }).ToList();

            var result = await _commandRepository.SaveQuotationsAsync(request.FreightRfqId, rows);

            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "FREIGHTRFQ_QUOTATIONS_SAVE",
                actionName: request.FreightRfqId.ToString(),
                details: $"Freight RFQ {request.FreightRfqId} quotations saved ({rows.Count} rows).",
                module: "FreightRfq"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Quotations saved successfully.",
                Data = result
            };
        }
    }
}
