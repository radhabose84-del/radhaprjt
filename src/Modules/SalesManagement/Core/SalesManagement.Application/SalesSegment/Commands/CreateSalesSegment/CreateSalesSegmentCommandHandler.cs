#nullable disable

using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesSegment.Commands.CreateSalesSegment
{
    public class CreateSalesSegmentCommandHandler : IRequestHandler<CreateSalesSegmentCommand, ApiResponseDTO<int>>
    {
        private readonly ISalesSegmentCommandRepository _commandRepository;
        private readonly ISalesSegmentQueryRepository _queryRepository;
        private readonly ICurrencyLookup _currencyLookup;
        private readonly IMediator _mediator;

        public CreateSalesSegmentCommandHandler(
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

        public async Task<ApiResponseDTO<int>> Handle(CreateSalesSegmentCommand request, CancellationToken cancellationToken)
        {
            // Validate composite key uniqueness
            var exists = await _queryRepository.CompositeKeyExistsAsync(
                request.SalesOrganisationId,
                request.SalesChannelId,
                request.BusinessUnitId);

            if (exists)
            {
                return new ApiResponseDTO<int>
                {
                    IsSuccess = false,
                    Message = "This combination of Sales Organisation, Sales Channel, and Business Unit already exists.",
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

            // Create entity
            var entity = new Domain.Entities.SalesSegment
            {
                SalesOrganisationId = request.SalesOrganisationId,
                SalesChannelId = request.SalesChannelId,
                BusinessUnitId = request.BusinessUnitId,
                CurrencyId = request.CurrencyId,
                ValidFrom = request.ValidFrom,
                SegmentName = request.SegmentName
            };

            var newId = await _commandRepository.CreateAsync(entity);

            // Publish audit event
            var auditEvent = new AuditLogsDomainEvent(
                actionDetail: "Create",
                actionCode: "SALES_SEGMENT_CREATE",
                actionName: request.SegmentName,
                details: $"Sales Segment '{request.SegmentName}' created successfully with Id {newId}.",
                module: "SalesSegment"
            );
            await _mediator.Publish(auditEvent, cancellationToken);

            return new ApiResponseDTO<int>
            {
                IsSuccess = true,
                Message = "Sales Segment created successfully.",
                Data = newId
            };
        }
    }
}
