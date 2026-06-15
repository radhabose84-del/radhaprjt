using FinanceManagement.Application.AccountGroup.Dto;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupById
{
    public class GetAccountGroupByIdQueryHandler : IRequestHandler<GetAccountGroupByIdQuery, AccountGroupDetailDto?>
    {
        private readonly IAccountGroupQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAccountGroupByIdQueryHandler(
            IAccountGroupQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<AccountGroupDetailDto?> Handle(GetAccountGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _queryRepository.GetByIdAsync(request.Id);

            if (result == null)
                return null;

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetById",
                actionCode: "GetAccountGroupByIdQuery",
                actionName: result.Id.ToString(),
                details: $"Account Group details {result.Id} was fetched.",
                module: "AccountGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return result;
        }
    }
}
