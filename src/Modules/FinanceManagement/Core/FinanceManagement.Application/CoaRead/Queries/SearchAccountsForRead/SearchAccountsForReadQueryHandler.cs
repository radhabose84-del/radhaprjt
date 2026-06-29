using Contracts.Common;
using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICoaRead;
using FinanceManagement.Application.CoaRead.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.CoaRead.Queries.SearchAccountsForRead
{
    public class SearchAccountsForReadQueryHandler : IRequestHandler<SearchAccountsForReadQuery, ApiResponseDTO<List<CoaAccountReadDto>>>
    {
        private readonly ICoaReadQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IMediator _mediator;

        public SearchAccountsForReadQueryHandler(
            ICoaReadQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<CoaAccountReadDto>>> Handle(SearchAccountsForReadQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var rows = await _queryRepository.SearchByTypeGroupAsync(
                companyId, request.AccountTypeId, request.AccountGroupId, request.ActiveOnly, cancellationToken);

            await _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "SearchAccountsForReadQuery",
                actionCode: "Get",
                actionName: rows.Count.ToString(),
                details: "COA read search by type/group.",
                module: "CoaRead"), cancellationToken);

            return new ApiResponseDTO<List<CoaAccountReadDto>>
            {
                IsSuccess = true,
                Message = "Success",
                Data = rows,
                TotalCount = rows.Count
            };
        }
    }
}
