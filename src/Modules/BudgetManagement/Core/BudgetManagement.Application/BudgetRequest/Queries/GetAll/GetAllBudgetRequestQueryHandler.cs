using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Projects;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using MediatR;

namespace BudgetManagement.Application.BudgetRequest.Queries.GetAll;

public class GetBudgetRequestListQueryHandler
    : IRequestHandler<GetAllBudgetRequestQuery, (IReadOnlyList<BudgetRequestListItemDto>, int)>
{
    private readonly IBudgetRequestQueryRepository _repo;
    private readonly IUnitLookup _unitLookup;
    private readonly ICurrencyLookup _currencyLookup;
    private readonly IFinancialYearLookup _financialYearLookup;
    private readonly IProjectLookup _projectLookup;
    private readonly IProjectWbsLookup _projectWbsLookup;

    public GetBudgetRequestListQueryHandler(
        IBudgetRequestQueryRepository repo,
        IUnitLookup unitLookup,
        ICurrencyLookup currencyLookup,
        IFinancialYearLookup financialYearLookup,
        IProjectLookup projectLookup,
        IProjectWbsLookup projectWbsLookup)
    {
        _repo = repo;
        _unitLookup = unitLookup;
        _currencyLookup = currencyLookup;
        _financialYearLookup = financialYearLookup;
        _projectLookup = projectLookup;
        _projectWbsLookup = projectWbsLookup;
    }

    public async Task<(IReadOnlyList<BudgetRequestListItemDto>, int)> Handle(
        GetAllBudgetRequestQuery request,
        CancellationToken ct)
    {
        var (items, total) = await _repo.GetAllAsync(
            request.StatusId,
            request.PageNumber,
            request.PageSize,
            request.SearchTerm,
            ct);

        if (!items.Any())
            return (items, total);

        // Collect IDs for lookup
        var unitIds = items.Select(x => x.UnitId).Distinct().ToList();
        var currencyIds = items.Select(x => x.CurrencyId).Where(id => id > 0).Distinct().ToList();
        var finYearIds = items.Select(x => x.FinancialYearId).Where(id => id > 0).Distinct().ToList();
        var projectIds = items.Where(x => x.ProjectId.HasValue && x.ProjectId > 0).Select(x => x.ProjectId!.Value).Distinct().ToList();
        var wbsIds = items.Where(x => x.WBSId.HasValue && x.WBSId > 0).Select(x => x.WBSId!.Value).Distinct().ToList();

        // Fetch lookups in parallel
        var unitsTask = _unitLookup.GetByIdsAsync(unitIds, ct);
        var currenciesTask = currencyIds.Any() ? _currencyLookup.GetByIdsAsync(currencyIds, ct) : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Users.CurrencyLookupDto>>(Array.Empty<Contracts.Dtos.Lookups.Users.CurrencyLookupDto>());
        var finYearsTask = finYearIds.Any() ? _financialYearLookup.GetByIdsAsync(finYearIds, ct) : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Users.FinancialYearLookupDto>>(Array.Empty<Contracts.Dtos.Lookups.Users.FinancialYearLookupDto>());
        var projectsTask = projectIds.Any() ? _projectLookup.GetByIdsAsync(projectIds, ct) : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Projects.ProjectLookupDto>>(Array.Empty<Contracts.Dtos.Lookups.Projects.ProjectLookupDto>());
        var wbsTask = wbsIds.Any() ? _projectWbsLookup.GetByIdsAsync(wbsIds, ct) : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Projects.ProjectWbsLookupDto>>(Array.Empty<Contracts.Dtos.Lookups.Projects.ProjectWbsLookupDto>());

        await Task.WhenAll(unitsTask, currenciesTask, finYearsTask, projectsTask, wbsTask);

        var unitLookupDict = (await unitsTask).ToDictionary(u => u.UnitId, u => (u.ShortName ?? u.UnitName ?? string.Empty).Trim());
        var currencyLookupDict = (await currenciesTask).ToDictionary(c => c.CurrencyId, c => c.Code ?? c.Name ?? string.Empty);
        var finYearLookupDict = (await finYearsTask).Where(fy => !string.IsNullOrWhiteSpace(fy.FinancialYearName)).ToDictionary(fy => fy.FinancialYearId, fy => fy.FinancialYearName!);
        var projectLookupDict = (await projectsTask).Where(p => !string.IsNullOrWhiteSpace(p.ProjectName)).ToDictionary(p => p.ProjectId, p => p.ProjectName!);
        var wbsLookupDict = (await wbsTask).Where(w => !string.IsNullOrWhiteSpace(w.WorkBreakdownStructureName)).ToDictionary(w => w.WbsId, w => w.WorkBreakdownStructureName!);

        // Enrich items
        foreach (var item in items)
        {
            if (unitLookupDict.TryGetValue(item.UnitId, out var unitName))
                item.UnitName = unitName;

            if (currencyLookupDict.TryGetValue(item.CurrencyId, out var currencyName))
                item.CurrencyName = currencyName;

            if (finYearLookupDict.TryGetValue(item.FinancialYearId, out var finYearName))
                item.FinancialYearName = finYearName;
            else if (item.FinancialYearId > 0)
                item.FinancialYearName = $"FY{item.FinancialYearId}";

            if (item.ProjectId.HasValue && projectLookupDict.TryGetValue(item.ProjectId.Value, out var projectName))
                item.ProjectName = projectName;

            if (item.WBSId.HasValue && wbsLookupDict.TryGetValue(item.WBSId.Value, out var wbsName))
                item.WBSName = wbsName;
        }

        return (items, total);
    }
}