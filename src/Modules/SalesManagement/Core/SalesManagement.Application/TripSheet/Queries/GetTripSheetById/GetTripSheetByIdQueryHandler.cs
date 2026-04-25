using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Application.TripSheet.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.TripSheet.Queries.GetTripSheetById
{
    public class GetTripSheetByIdQueryHandler : IRequestHandler<GetTripSheetByIdQuery, TripSheetHeaderDto?>
    {
        private readonly ITripSheetQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetTripSheetByIdQueryHandler(ITripSheetQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<TripSheetHeaderDto?> Handle(GetTripSheetByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetTripSheetByIdQuery",
                actionName: result.Id.ToString(),
                details: $"TripSheet details {result.Id} was fetched.",
                module: "TripSheet"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
