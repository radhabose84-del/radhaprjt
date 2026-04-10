
using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationValue.Queries.GetItemSpecificationValueById
{
    public class GetItemSpecificationValueByIdQueryHandler : IRequestHandler<GetItemSpecificationValueByIdQuery, ItemSpecificationValueDto?>
    {
        private readonly IItemSpecificationValueQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetItemSpecificationValueByIdQueryHandler(
            IItemSpecificationValueQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ItemSpecificationValueDto?> Handle(GetItemSpecificationValueByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result is null)
                return null;

            var dto = _mapper.Map<ItemSpecificationValueDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetItemSpecificationValueByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Item Specification Value details {dto.Id} was fetched.",
                module: "ItemSpecificationValue"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
