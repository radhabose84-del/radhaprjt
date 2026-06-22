using FinanceManagement.Application.CoaChangeRequest.Dto;
using FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CoaChangeRequest.Queries.GetCoaUnfreezeRequestById
{
    public class GetCoaUnfreezeRequestByIdQueryHandler
        : IRequestHandler<GetCoaUnfreezeRequestByIdQuery, CoaUnfreezeRequestDto?>
    {
        private readonly ICoaChangeRequestQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetCoaUnfreezeRequestByIdQueryHandler(
            ICoaChangeRequestQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<CoaUnfreezeRequestDto?> Handle(
            GetCoaUnfreezeRequestByIdQuery request, CancellationToken cancellationToken)
        {
            var dto = await _queryRepository.GetUnfreezeRequestByIdAsync(request.Id, cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetCoaUnfreezeRequestByIdQuery",
                actionName: request.Id.ToString(),
                details: $"COA unfreeze request {request.Id} was fetched.",
                module: "CoaChangeRequest"), cancellationToken);

            return dto;
        }
    }
}
