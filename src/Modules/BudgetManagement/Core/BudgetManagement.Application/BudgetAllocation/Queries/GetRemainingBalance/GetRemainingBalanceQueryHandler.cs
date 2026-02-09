using Contracts.Interfaces.Lookups.Users;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BudgetManagement.Application.BudgetAllocation.Queries.GetRemainingBalance;

public sealed class GetRemainingBalanceQueryHandler
    : IRequestHandler<GetRemainingBalanceQuery, RemainingBalanceWithPrevDto>
{
    private readonly IBudgetAllocationQueryRepository _repo;
    private readonly IFinancialYearLookup _financialYearLookup;
    private readonly ILogger<GetRemainingBalanceQueryHandler> _logger;

    public GetRemainingBalanceQueryHandler(IBudgetAllocationQueryRepository repo, IFinancialYearLookup financialYearLookup, ILogger<GetRemainingBalanceQueryHandler> logger)
    {
        _repo = repo;
        _financialYearLookup = financialYearLookup;
        _logger = logger;
    }

    public async Task<RemainingBalanceWithPrevDto> Handle(GetRemainingBalanceQuery request, CancellationToken ct)
    {
        var requestDate = request.BudgetDate ?? DateOnly.FromDateTime(DateTime.Today);
        if (request.FinancialYearId <= 0 || request.FinancialYearId == null)
        {
            var financialYears = await _financialYearLookup
                .GetAllFinancialYearAsync();

            // convert DateTime → DateOnly for comparison
            var fyMatch = financialYears
                .FirstOrDefault(fy =>
                {
                    var start = DateOnly.FromDateTime(fy.StartDate.Date);
                    var end = DateOnly.FromDateTime(fy.EndDate.Date);
                    return start <= requestDate && requestDate <= end;
                });

            if (fyMatch != null)
            {
                request.FinancialYearId = fyMatch.FinancialYearId;
            }
            else
            {
                var msg = $"Financial year does not exist for date {requestDate:yyyy-MM-dd}.";
                _logger.LogWarning(msg);

                // Example if you have custom ValidationException
                throw new ApplicationException(msg);
            }
        }
        return await _repo.GetRemainingBalanceAsync(
            request.BudgetGroupId,
            request.BudgetDate,
            request.MonthId,
            request.RequestById,
            request.ProjectId,
            request.WbsId, request.FinancialYearId,
            ct);
    }
}
