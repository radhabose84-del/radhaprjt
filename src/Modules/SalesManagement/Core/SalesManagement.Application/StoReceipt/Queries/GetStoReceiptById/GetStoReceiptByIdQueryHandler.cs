using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Application.StoReceipt.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.StoReceipt.Queries.GetStoReceiptById
{
    public class GetStoReceiptByIdQueryHandler : IRequestHandler<GetStoReceiptByIdQuery, StoReceiptHeaderDto?>
    {
        private readonly IStoReceiptQueryRepository _queryRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetStoReceiptByIdQueryHandler(
            IStoReceiptQueryRepository queryRepository,
            IMapper mapper,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<StoReceiptHeaderDto?> Handle(GetStoReceiptByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetStoReceiptByIdQuery",
                actionName: result.Id.ToString(),
                details: $"STO Receipt details {result.Id} was fetched.",
                module: "StoReceipt");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
