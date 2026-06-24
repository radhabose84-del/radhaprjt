using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.ILedgerBalance;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Domain.Events;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.LedgerBalance.Queries.GetAllLedgerBalance
{
    public class GetAllLedgerBalanceQueryHandler : IRequestHandler<GetAllLedgerBalanceQuery, ApiResponseDTO<List<LedgerBalanceDto>>>
    {
        private readonly ILedgerBalanceQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;
        private readonly IFinancialYearLookup _financialYearLookup;
        private readonly IMediator _mediator;

        public GetAllLedgerBalanceQueryHandler(
            ILedgerBalanceQueryRepository queryRepository,
            IIPAddressService ipAddressService,
            IFinancialYearLookup financialYearLookup,
            IMediator mediator)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
            _financialYearLookup = financialYearLookup;
            _mediator = mediator;
        }

        public async Task<ApiResponseDTO<List<LedgerBalanceDto>>> Handle(GetAllLedgerBalanceQuery request, CancellationToken cancellationToken)
        {
            var companyId = _ipAddressService.GetCompanyId()
                ?? throw new ExceptionRules("No active company in session.");

            var (data, totalCount) = await _queryRepository.GetAllAsync(
                request.PageNumber, request.PageSize, companyId,
                request.AccountingPeriodId, request.FinancialYearId, request.GlAccountId,
                request.AccountTypeId, request.AccountGroupId, request.CostCentreId, request.SearchTerm);

            // Financial year is cross-module → resolve its name via the (cached) lookup.
            if (data.Count > 0)
            {
                var years = await _financialYearLookup.GetAllFinancialYearAsync();
                var yearName = years.ToDictionary(y => y.FinancialYearId, y => y.FinancialYearName);
                foreach (var row in data)
                    row.FinancialYearName = yearName.TryGetValue(row.FinancialYearId, out var n) ? n : null;
            }

            var domainEvent = new AuditLogsDomainEvent(
                actionDetail: "GetAllLedgerBalanceQuery",
                actionCode: "Get",
                actionName: data.Count.ToString(),
                details: "Ledger balances were fetched.",
                module: "LedgerBalance"
            );
            await _mediator.Publish(domainEvent, cancellationToken);

            return new ApiResponseDTO<List<LedgerBalanceDto>>
            {
                IsSuccess = true,
                Message = "Ledger balances retrieved successfully.",
                Data = data,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };
        }
    }
}
