using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.MarketingOfficer.Queries.GetMarketingOfficerById
{
    public class GetMarketingOfficerByIdQueryHandler : IRequestHandler<GetMarketingOfficerByIdQuery, MarketingOfficerDto?>
    {
        private readonly IMarketingOfficerQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetMarketingOfficerByIdQueryHandler(IMarketingOfficerQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<MarketingOfficerDto?> Handle(GetMarketingOfficerByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<MarketingOfficerDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetMarketingOfficerByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"MarketingOfficer details {dto.Id} was fetched.",
                module: "MarketingOfficer"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
