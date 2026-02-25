#nullable disable
using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesGroup.Queries.GetSalesGroupById
{
    public class GetSalesGroupByIdQueryHandler : IRequestHandler<GetSalesGroupByIdQuery, SalesGroupDto>
    {
        private readonly ISalesGroupQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesGroupByIdQueryHandler(ISalesGroupQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<SalesGroupDto> Handle(GetSalesGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<SalesGroupDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetSalesGroupByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"SalesGroup details {dto.Id} was fetched.",
                module: "SalesGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}