using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesContact;
using SalesManagement.Application.SalesContact.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesContact.Queries.GetSalesContactById
{
    public class GetSalesContactByIdQueryHandler : IRequestHandler<GetSalesContactByIdQuery, SalesContactDto?>
    {
        private readonly ISalesContactQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesContactByIdQueryHandler(
            ISalesContactQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<SalesContactDto?> Handle(GetSalesContactByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var dto = _mapper.Map<SalesContactDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetSalesContactByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Sales Contact details {dto.Id} was fetched.",
                module: "SalesContact"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
