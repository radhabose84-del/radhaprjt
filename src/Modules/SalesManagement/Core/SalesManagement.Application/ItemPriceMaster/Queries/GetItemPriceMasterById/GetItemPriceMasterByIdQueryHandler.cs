using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.ItemPriceMaster.Queries.GetItemPriceMasterById
{
    public class GetItemPriceMasterByIdQueryHandler
        : IRequestHandler<GetItemPriceMasterByIdQuery, ItemPriceMasterDto?>
    {
        private readonly IItemPriceMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetItemPriceMasterByIdQueryHandler(IItemPriceMasterQueryRepository queryRepository, IMapper mapper, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ItemPriceMasterDto?> Handle(
            GetItemPriceMasterByIdQuery request,
            CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var salesItemPriceMaster = _mapper.Map<ItemPriceMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetItemPriceMasterByIdQuery",
                actionName: salesItemPriceMaster.Id.ToString(),
                details: $"ItemPriceMaster details {salesItemPriceMaster.Id} was fetched.",
                module: "ItemPriceMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return salesItemPriceMaster;
        }
    }
}