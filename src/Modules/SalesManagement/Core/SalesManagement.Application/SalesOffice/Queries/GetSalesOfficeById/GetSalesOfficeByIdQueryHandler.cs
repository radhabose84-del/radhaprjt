#nullable disable
using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOffice.Queries.GetSalesOfficeById
{
    public class GetSalesOfficeByIdQueryHandler : IRequestHandler<GetSalesOfficeByIdQuery, SalesOfficeDto>
    {
        private readonly ISalesOfficeQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesOfficeByIdQueryHandler(ISalesOfficeQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<SalesOfficeDto> Handle(GetSalesOfficeByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var salesOffice = _mapper.Map<SalesOfficeDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetSalesOfficeByIdQuery",
                actionName: salesOffice.Id.ToString(),
                details: $"SalesOffice details {salesOffice.Id} was fetched.",
                module: "SalesOffice"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return salesOffice;
        }
    }
}