using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetGstrSectionAccountLinkageById
{
    public class GetGstrSectionAccountLinkageByIdQueryHandler : IRequestHandler<GetGstrSectionAccountLinkageByIdQuery, GstrSectionAccountLinkageDto?>
    {
        private readonly IGstrSectionQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetGstrSectionAccountLinkageByIdQueryHandler(IGstrSectionQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<GstrSectionAccountLinkageDto?> Handle(GetGstrSectionAccountLinkageByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetLinkageByIdAsync(request.Id);
            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetGstrSectionAccountLinkageByIdQuery",
                actionName: result.Id.ToString(),
                details: $"GSTR section-account mapping {result.Id} was fetched.",
                module: "GstrSectionAccountLinkage"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
