using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.Quotations.QuotationEntry.Queries.GetQuotationAutoComplete;

public class GetQuotationAutoCompleteQueryHandler
    : IRequestHandler<GetQuotationAutoCompleteQuery, List<QuotationAutoCompleteDto>>
{
    private readonly IQuotationQueryRepository _repo;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public GetQuotationAutoCompleteQueryHandler(IQuotationQueryRepository repo, IMapper mapper, IMediator mediator)
    {
        _repo = repo;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<List<QuotationAutoCompleteDto>> Handle(GetQuotationAutoCompleteQuery request, CancellationToken cancellationToken)
    {
        var search = request.SearchPattern?.Trim();
        var result = await _repo.GetQuotationAutoComplete(search);
        var items = _mapper.Map<List<QuotationAutoCompleteDto>>(result);
          //Domain Event
        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "GetAll",
            actionCode: "GetQuotationAutoCompleteQuery",        
            actionName: items.Count.ToString(),
            details: $"Quotation details was fetched.",
            module:"QuotationEntry"
        );
        await _mediator.Publish(domainEvent, cancellationToken);
        return items;        
    }
}
