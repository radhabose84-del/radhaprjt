using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Application.StoReceipt.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoReceipt.Queries.GetDcOpenQty
{
    public class GetDcOpenQtyQueryHandler : IRequestHandler<GetDcOpenQtyQuery, DcOpenQtyDto?>
    {
        private readonly IStoReceiptQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetDcOpenQtyQueryHandler(
            IStoReceiptQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<DcOpenQtyDto?> Handle(GetDcOpenQtyQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetDcOpenQtyAsync(request.DcDetailId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetDcOpenQty",
                actionCode: "GetDcOpenQtyQuery",
                actionName: request.DcDetailId.ToString(),
                details: $"DC open quantity for detail {request.DcDetailId} was fetched.",
                module: "StoReceipt");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
