using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder;
using PurchaseManagement.Application.PurchaseOrder.Print.Dto;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.Application.PurchaseOrder.Print.Queries.GetPurchaseOrderPrintDetails;

public class GetPurchaseOrderPrintDetailsQueryHandler : IRequestHandler<GetPurchaseOrderPrintDetailsQuery, PurchaseOrderPrintDto?>
{
    private readonly IPurchaseOrderPrintQueryRepository _queryRepository;
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public GetPurchaseOrderPrintDetailsQueryHandler(
        IPurchaseOrderPrintQueryRepository queryRepository,
        IMapper mapper,
        IMediator mediator)
    {
        _queryRepository = queryRepository;
        _mapper = mapper;
        _mediator = mediator;
    }

    public async Task<PurchaseOrderPrintDto?> Handle(GetPurchaseOrderPrintDetailsQuery request, CancellationToken cancellationToken)
    {
        var result = await _queryRepository.GetPrintDetailsAsync(request.Id, cancellationToken);

        if (result == null)
            return null;

        var domainEvent = new AuditLogsDomainEvent(
            actionDetail: "GetPrintDetails",
            actionCode: "PO_PRINT",
            actionName: request.Id.ToString(),
            details: $"Purchase Order print details {request.Id} was fetched.",
            module: "PurchaseOrder");
        await _mediator.Publish(domainEvent, cancellationToken);

        return result;
    }
}
