
using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Dto;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.Application.ItemSpecificationMaster.Queries.GetItemSpecificationMasterById
{
    public class GetItemSpecificationMasterByIdQueryHandler : IRequestHandler<GetItemSpecificationMasterByIdQuery, ItemSpecificationMasterDto?>
    {
        private readonly IItemSpecificationMasterQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetItemSpecificationMasterByIdQueryHandler(
            IItemSpecificationMasterQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<ItemSpecificationMasterDto?> Handle(GetItemSpecificationMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result is null)
                return null;

            var dto = _mapper.Map<ItemSpecificationMasterDto>(result);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetItemSpecificationMasterByIdQuery",
                actionName: dto.Id.ToString(),
                details: $"Item Specification Master details {dto.Id} was fetched.",
                module: "ItemSpecificationMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return dto;
        }
    }
}
