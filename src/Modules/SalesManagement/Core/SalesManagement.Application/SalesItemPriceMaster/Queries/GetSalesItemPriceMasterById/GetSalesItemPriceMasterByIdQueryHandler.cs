using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesItemPriceMaster;
using SalesManagement.Application.SalesItemPriceMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesItemPriceMaster.Queries.GetSalesItemPriceMasterById
{
    public class GetSalesItemPriceMasterByIdQueryHandler
        : IRequestHandler<GetSalesItemPriceMasterByIdQuery, SalesItemPriceMasterDto?>
    {
        private readonly ISalesItemPriceMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetSalesItemPriceMasterByIdQueryHandler(ISalesItemPriceMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<SalesItemPriceMasterDto?> Handle(
            GetSalesItemPriceMasterByIdQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var salesItemPriceMaster = _mapper.Map<SalesItemPriceMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetSalesItemPriceMasterByIdQuery",
                actionName: salesItemPriceMaster.Id.ToString(),
                details: $"SalesItemPriceMaster details {salesItemPriceMaster.Id} was fetched.",
                module: "SalesItemPriceMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return salesItemPriceMaster;
        }
    }
}