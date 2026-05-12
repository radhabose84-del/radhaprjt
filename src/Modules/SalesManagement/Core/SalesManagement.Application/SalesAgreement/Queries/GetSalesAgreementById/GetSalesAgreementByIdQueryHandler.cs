using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesAgreement;
using SalesManagement.Application.SalesAgreement.Dto;
using SalesManagement.Domain.Events;

namespace SalesManagement.Application.SalesAgreement.Queries.GetSalesAgreementById
{
    public class GetSalesAgreementByIdQueryHandler : IRequestHandler<GetSalesAgreementByIdQuery, SalesAgreementHeaderDto?>
    {
        private readonly ISalesAgreementQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetSalesAgreementByIdQueryHandler(
            ISalesAgreementQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<SalesAgreementHeaderDto?> Handle(GetSalesAgreementByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetSalesAgreementByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Sales Agreement details {result.Id} was fetched.",
                module: "SalesAgreement");
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
