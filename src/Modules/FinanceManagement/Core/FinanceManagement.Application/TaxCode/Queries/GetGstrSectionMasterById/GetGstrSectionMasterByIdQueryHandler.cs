using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IGstrSection;
using FinanceManagement.Application.TaxCode.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.TaxCode.Queries.GetGstrSectionMasterById
{
    public class GetGstrSectionMasterByIdQueryHandler : IRequestHandler<GetGstrSectionMasterByIdQuery, GstrSectionMasterDto?>
    {
        private readonly IGstrSectionQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetGstrSectionMasterByIdQueryHandler(IGstrSectionQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<GstrSectionMasterDto?> Handle(GetGstrSectionMasterByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetSectionByIdAsync(request.Id);
            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetGstrSectionMasterByIdQuery",
                actionName: result.Id.ToString(),
                details: $"GSTR section {result.Id} was fetched.",
                module: "GstrSectionMaster"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
