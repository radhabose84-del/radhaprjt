using AutoMapper;
using MediatR;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Dto;
using ProductionManagement.Domain.Events;

namespace ProductionManagement.Application.RepackingHeader.Queries.GetRepackingHeaderById
{
    public class GetRepackingHeaderByIdQueryHandler
        : IRequestHandler<GetRepackingHeaderByIdQuery, RepackingHeaderDto?>
    {
        private readonly IRepackingHeaderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetRepackingHeaderByIdQueryHandler(
            IRepackingHeaderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<RepackingHeaderDto?> Handle(
            GetRepackingHeaderByIdQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetRepackingHeaderByIdQuery",
                actionName: result.Id.ToString(),
                details: $"RepackingHeader details {result.Id} was fetched.",
                module: "RepackingHeader"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
