#nullable disable

using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.Application.SalesSegment.Commands.UpdateSalesSegment
{
    public class UpdateSalesSegmentCommandHandler : IRequestHandler<UpdateSalesSegmentCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesSegmentCommandRepository _commandRepository;
        private readonly ISalesSegmentQueryRepository _queryRepository;
        private readonly ICurrencyLookup _currencyLookup;
        private readonly IMediator _mediator;

        public UpdateSalesSegmentCommandHandler(
            ISalesSegmentCommandRepository commandRepository,
            ISalesSegmentQueryRepository queryRepository,
            ICurrencyLookup currencyLookup,
            IMediator mediator)
        {
            _commandRepository = commandRepository;
            _queryRepository = queryRepository;
            _currencyLookup = currencyLookup;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<int>> Handle(UpdateSalesSegmentCommand request, CancellationToken cancellationToken)
        {
            // Validate entity exists
            var notFound = await _queryRepository.NotFoundAsync(request.Id);
            if (notFound)
            {
                return new ApiResponseDTO<int>
                {
                    IsSuccess = false,
                    Message = "Sales Segment not found.",
                    Data = 0
                };
            }

            // Validate CurrencyId if provided
            if (request.CurrencyId.HasValue)
            {
                var currencies = await _currencyLookup.GetByIdsAsync(new[] { request.CurrencyId.Value }, cancellationToken);
                var currencyExists = currencies.Any();

                if (!currencyExists)
                {
                    return new ApiResponseDTO<int>
                    {
                        IsSuccess = false,
                        Message = "Currency does not exist.",
                        Data = 0
                    };
                }
            }

            // Create entity with updated values
            var entity = new Domain.Entities.SalesSegment
            {
                Id = request.Id,
                CurrencyId = request.CurrencyId,
                ValidFrom = request.ValidFrom,
                SegmentName = request.SegmentName,
                IsActive = request.IsActive == 1 ? Status.Active : Status.Inactive
            };

            var result = await _commandRepository.UpdateAsync(entity);

            // Publish audit event
            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Update",
                actionCode: "SALES_SEGMENT_UPDATE",
                actionName: request.SegmentName,
                details: $"Sales Segment Id {request.Id} updated successfully.",
                module: "SalesSegment"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Segment updated successfully.",
                Data = result
            };
        }
    }
}
