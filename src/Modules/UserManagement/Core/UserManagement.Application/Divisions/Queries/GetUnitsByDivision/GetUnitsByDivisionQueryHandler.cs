using MediatR;
using UserManagement.Application.Common.Interfaces.IDivision;
using UserManagement.Domain.Events;

namespace UserManagement.Application.Divisions.Queries.GetUnitsByDivision
{
    public class GetUnitsByDivisionQueryHandler : IRequestHandler<GetUnitsByDivisionQuery, List<GetUnitsByDivisionDto>>
    {
        private readonly IDivisionQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetUnitsByDivisionQueryHandler(IDivisionQueryRepository queryRepository, IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<List<GetUnitsByDivisionDto>> Handle(GetUnitsByDivisionQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetUnitsByDivisionAsync(request.CompanyId, request.DivisionId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetUnitsByDivision",
                actionCode: "DIVISION_UNITS_GET",
                actionName: request.DivisionId.ToString(),
                details: $"Units for DivisionId {request.DivisionId} in CompanyId {request.CompanyId} fetched. Count: {result.Count}.",
                module: "Division"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
