using AutoMapper;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Application.Quotations.QuotationEntry.DTOs;
using PurchaseManagement.Domain.Events;
using MediatR;

namespace PurchaseManagement.Application.Quotations.QuotationEntry.Queries.GetQuotationById;

public class GetQuotationByIdQueryHandler
    : IRequestHandler<GetQuotationByIdQuery, GetQuotationHeaderDto>
{
    private readonly IQuotationQueryRepository _repo;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public GetQuotationByIdQueryHandler(IQuotationQueryRepository repo, IMapper mapper, IMediator mediator)
    {
        _repo = repo;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<GetQuotationHeaderDto> Handle(GetQuotationByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await _repo.GetByIdAsync(request.Id, cancellationToken)
                  ?? throw new KeyNotFoundException("Quotation not found.");
        var result = _mapper.Map<GetQuotationHeaderDto>(dto);
        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "GetById",
            actionCode: string.Empty,
            actionName: string.Empty,
            details: $"Quotation details {result.Id} was fetched.",
            module: "Quotation");
        await _mediator.Publish(domainEvent, cancellationToken);
        return result;
    }
}

