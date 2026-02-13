using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.DTOs;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.Quotation.RfqEntry.Queries.GetAllRfq
{
    public class GetAllRfqQueryHandler 
        : IRequestHandler<GetAllRfqQuery, (IReadOnlyList<RfqListItemDto> Items, int Total)>
    {
        private readonly IRfqQueryRepository _repo;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetAllRfqQueryHandler(
            IRfqQueryRepository repo, 
            IMapper mapper, 
            IMediator mediator)
            => (_repo, _mapper, _mediator) = (repo, mapper, mediator);

        public async Task<(IReadOnlyList<RfqListItemDto> Items, int Total)> Handle(
            GetAllRfqQuery request, 
            CancellationToken ct)
        {
            var (items, total) = await _repo.GetAllAsync(
                statusId:  request.StatusId,
                page:      request.PageNumber,
                pageSize:  request.PageSize,
                searchTerm: request.SearchTerm,
                ct:        ct);

            // map if you really need AutoMapper; otherwise you can return items directly
            var result = _mapper.Map<List<RfqListItemDto>>(items);

            // Domain Event
            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAll",
                actionCode:   "GetAllRfqQuery",
                actionName:   total.ToString(), // logging total count
                details:      $"RFQ list fetched. Page={request.PageNumber}, Size={request.PageSize}, Total={total}",
                module:       "GetAllRFQ"
            );

            await _mediator.Publish(domainEvent, ct);

            return (result, total);
        }
    }
}
