using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Application.CustomerVisit.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.CustomerVisit.Queries.GetCustomerVisitById
{
    public class GetCustomerVisitByIdQueryHandler : IRequestHandler<GetCustomerVisitByIdQuery, CustomerVisitDto?>
    {
        private readonly ICustomerVisitQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetCustomerVisitByIdQueryHandler(ICustomerVisitQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<CustomerVisitDto?> Handle(GetCustomerVisitByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<CustomerVisitDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetCustomerVisitByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"CustomerVisit details {dto.Id} was fetched.",
                module: "CustomerVisit"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
