using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrder;
using SalesManagement.Application.SalesOrder.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesOrder.Queries.GetSalesOrderAutoComplete
{
    public class GetSalesOrderAutoCompleteQueryHandler : IRequestHandler<GetSalesOrderAutoCompleteQuery, IReadOnlyList<SalesOrderLookupDto>>
    {
        private readonly ISalesOrderQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesOrderAutoCompleteQueryHandler(
            ISalesOrderQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<IReadOnlyList<SalesOrderLookupDto>> Handle(
            GetSalesOrderAutoCompleteQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.AutocompleteAsync(request.Term ?? string.Empty, cancellationToken, request.ProformaFilter);
            var dtos = _mapper.Map<List<SalesOrderLookupDto>>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode: "GetSalesOrderAutoCompleteQuery",
                actionName: dtos.Count.ToString(),
                details: "SalesOrder details was fetched.",
                module: "SalesOrder"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dtos;
        }
    }
}
