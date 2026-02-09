using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Workflow;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IBudgetRequest;
using BudgetManagement.Domain.Common;
using BudgetManagement.Domain.Events;
using MediatR;

namespace BudgetManagement.Application.BudgetRequest.Queries.GetBudgetRequestPending
{
    public sealed class GetBudgetRequestPendingQueryHandler
        : IRequestHandler<GetBudgetRequestPendingQuery, (List<GetBudgetRequestPendingDto> Items, int TotalCount)>
    {
        private readonly IBudgetRequestQueryRepository _repo;
        private readonly IMediator _mediator;
        //private readonly IWorkflowLookup _workflowLookup;
        private readonly IUserLookup _userLookup;
        private readonly IIPAddressService _ipAddressService;
        private readonly IUnitLookup _unitLookup;
        private readonly ICurrencyLookup _currencyLookup;
        private readonly IFinancialYearLookup _financialYearLookup;

        public GetBudgetRequestPendingQueryHandler(
            IBudgetRequestQueryRepository repo,
            IMediator mediator,
            //IWorkflowLookup workflowLookup,
            IUserLookup userLookup,
            IIPAddressService ipAddressService,
            IUnitLookup unitLookup,
            ICurrencyLookup currencyLookup,
            IFinancialYearLookup financialYearLookup)
        {
            _repo = repo;
            _mediator = mediator;
            //_workflowLookup = workflowLookup;
            _userLookup = userLookup;
            _ipAddressService = ipAddressService;
            _unitLookup = unitLookup;
            _currencyLookup = currencyLookup;
            _financialYearLookup = financialYearLookup;
        }

        public async Task<(List<GetBudgetRequestPendingDto> Items, int TotalCount)> Handle(
            GetBudgetRequestPendingQuery request, CancellationToken ct)
        {
            var page = request.PageNumber ?? 1;
            var size = request.PageSize ?? 15;

            // 1) Get pending rows from repo
            var (rows, totalFromRepo) = await _repo.GetPendingRequestAsync(
                page, size, request.SearchTerm, ct);

            if (rows.Count == 0)
            {
                await PublishAudit(0, request, ct);
                return (rows, 0);
            }

            // Enrich with lookup data
            var unitIds = rows.Select(x => x.UnitId).Where(id => id > 0).Distinct().ToList();
            var currencyIds = rows.Select(x => x.CurrencyId).Where(id => id > 0).Distinct().ToList();
            var finYearIds = rows.Select(x => x.FinancialYearId).Where(id => id > 0).Distinct().ToList();

            var unitsTask = unitIds.Any() ? _unitLookup.GetByIdsAsync(unitIds, ct) : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Users.UnitLookupDto>>(Array.Empty<Contracts.Dtos.Lookups.Users.UnitLookupDto>());
            var currenciesTask = currencyIds.Any() ? _currencyLookup.GetByIdsAsync(currencyIds, ct) : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Users.CurrencyLookupDto>>(Array.Empty<Contracts.Dtos.Lookups.Users.CurrencyLookupDto>());
            var finYearsTask = finYearIds.Any() ? _financialYearLookup.GetByIdsAsync(finYearIds, ct) : Task.FromResult<IReadOnlyList<Contracts.Dtos.Lookups.Users.FinancialYearLookupDto>>(Array.Empty<Contracts.Dtos.Lookups.Users.FinancialYearLookupDto>());

            await Task.WhenAll(unitsTask, currenciesTask, finYearsTask);

            var unitLookupDict = (await unitsTask).ToDictionary(u => u.UnitId, u => (u.ShortName ?? u.UnitName ?? string.Empty).Trim());
            var currencyLookupDict = (await currenciesTask).ToDictionary(c => c.CurrencyId, c => c.Code ?? c.Name ?? string.Empty);
            var finYearLookupDict = (await finYearsTask).Where(fy => !string.IsNullOrWhiteSpace(fy.FinancialYearName)).ToDictionary(fy => fy.FinancialYearId, fy => fy.FinancialYearName!);

            foreach (var r in rows)
            {
                if (unitLookupDict.TryGetValue(r.UnitId, out var unitName))
                    r.UnitName = unitName;

                if (currencyLookupDict.TryGetValue(r.CurrencyId, out var currencyName))
                    r.CurrencyName = currencyName;

                if (finYearLookupDict.TryGetValue(r.FinancialYearId, out var finYearName))
                    r.FinYear = finYearName;
                else if (r.FinancialYearId > 0)
                    r.FinYear = $"FY{r.FinancialYearId}";
            }

            // 2) Workflow – filter only those where current user is approver
            //var currentUserId = _ipAddressService.GetUserId();

            //var requestIds = rows
            //    .Select(r => r.Id)
            //    .Where(id => id > 0)
            //    .Distinct()
            //    .ToList();

            //var wfApprovers = await _workflowLookup.GetApproverListAsync(
            //    MiscEnumEntity.BudgetRequest.ToString(),
            //    requestIds);

            //var allowedRequestIds = wfApprovers
            //    .Where(a => !string.IsNullOrWhiteSpace(a.ApproverValue)
            //                && int.TryParse(a.ApproverValue, out var parsed)
            //                && parsed == currentUserId)
            //    .Select(a => a.ModuleTransactionId)
            //    .ToHashSet();

            //rows = rows
            //    .Where(r => allowedRequestIds.Contains(r.Id))
            //    .ToList();

            //if (rows.Count == 0)
            //{
            //    await PublishAudit(0, request, ct);
            //    return (rows, 0);
            //}

            //// 3) Map workflow details to rows (ApproverId, ApproverName, ApprovalRequestHeaderId)
            //var wfByRequestId = wfApprovers
            //    .Where(a => allowedRequestIds.Contains(a.ModuleTransactionId))
            //    .GroupBy(a => a.ModuleTransactionId)
            //    .ToDictionary(
            //        g => g.Key,
            //        g =>
            //        {
            //            var first = g.First();
            //            int? approverId = null;
            //            if (!string.IsNullOrWhiteSpace(first.ApproverValue) &&
            //                int.TryParse(first.ApproverValue, out var parsed))
            //            {
            //                approverId = parsed;
            //            }

            //            return new
            //            {
            //                ApproverId        = approverId,
            //                ApprovalRequestId = first.ApprovalRequestId
            //            };
            //        });

            //var users = await _userLookup.GetAllUserAsync();
            //var userLookupDict = users.ToDictionary(u => u.UserId, u => u.UserName);

            //foreach (var r in rows)
            //{
            //    if (wfByRequestId.TryGetValue(r.Id, out var wf))
            //    {
            //        if (wf.ApproverId.HasValue)
            //        {
            //            r.ApproverId = wf.ApproverId.Value;
            //            if (userLookupDict.TryGetValue(r.ApproverId.Value, out var approverName))
            //                r.ApproverName = approverName;
            //        }

            //        r.ApprovalRequestHeaderId = wf.ApprovalRequestId;
            //    }
            //}

            await PublishAudit(rows.Count, request, ct);
            return (rows, rows.Count);
        }


        private Task PublishAudit(int count, GetBudgetRequestPendingQuery q, CancellationToken ct)
            => _mediator.Publish(new AuditLogsDomainEvent(
                actionDetail: "GetAll-Pending",
                actionCode  : string.Empty,
                actionName  : "BudgetRequestPending",
                details     : $"Fetched {count} BudgetRequest pending rows. Page={q.PageNumber}, Size={q.PageSize}, Search='{q.SearchTerm ?? ""}'.",
                module      : "BudgetRequest"), ct);
    }
}
