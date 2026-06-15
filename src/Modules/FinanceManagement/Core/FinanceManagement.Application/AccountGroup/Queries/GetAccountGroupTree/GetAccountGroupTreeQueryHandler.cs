using Contracts.Common;
using FinanceManagement.Application.AccountGroup.Dto;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupTree
{
    public class GetAccountGroupTreeQueryHandler : IRequestHandler<GetAccountGroupTreeQuery, ApiResponseDTO<List<AccountGroupTreeDto>>>
    {
        private readonly IAccountGroupQueryRepository _queryRepository;
        private readonly IMediator _mediator;

        public GetAccountGroupTreeQueryHandler(
            IAccountGroupQueryRepository queryRepository,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<AccountGroupTreeDto>>> Handle(GetAccountGroupTreeQuery request, CancellationToken cancellationToken)
        {
            var tree = await _queryRepository.GetTreeAsync(request.CompanyId);

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAccountGroupTreeQuery",
                actionCode: "Get",
                actionName: tree.Count.ToString(),
                details: "Account Group hierarchy was fetched.",
                module: "AccountGroup"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<AccountGroupTreeDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = tree,
                TotalCount = tree.Count
            };
        }
    }
}
