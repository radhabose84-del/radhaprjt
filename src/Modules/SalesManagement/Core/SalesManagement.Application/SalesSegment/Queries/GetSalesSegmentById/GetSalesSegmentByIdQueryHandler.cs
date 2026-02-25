#nullable disable
using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces.Lookups.Users;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesSegment;
using SalesManagement.Application.SalesSegment.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesSegment.Queries.GetSalesSegmentById
{
    public class GetSalesSegmentByIdQueryHandler : IRequestHandler<GetSalesSegmentByIdQuery, ApiResponseDTO<SalesSegmentDto>>
    {
        private readonly ISalesSegmentQueryRepository _queryRepository;
        private readonly ICurrencyLookup _currencyLookup;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesSegmentByIdQueryHandler(
            ISalesSegmentQueryRepository queryRepository,
            ICurrencyLookup currencyLookup,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _currencyLookup = currencyLookup;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<SalesSegmentDto>> Handle(GetSalesSegmentByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            // Populate currency name via lookup
            if (result.CurrencyId.HasValue)
            {
                var currencies = await _currencyLookup.GetByIdsAsync(new[] { result.CurrencyId.Value }, cancellationToken);
                var currency = currencies.FirstOrDefault();
                result.CurrencyName = currency?.Name;
            }

            var salesSegment = _mapper.Map<SalesSegmentDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetSalesSegmentByIdQuery",
                actionName: salesSegment.Id.ToString(),
                details: $"SalesSegment details {salesSegment.Id} was fetched.",
                module: "SalesSegment"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<SalesSegmentDto>
            {
                IsSuccess = true,
                Message = "Sales Segment retrieved successfully.",
                Data = salesSegment
            };
        }
    }
}
