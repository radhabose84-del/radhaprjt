using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static FinanceManagement.Domain.Common.BaseEntity;

namespace BackgroundService.Infrastructure.Jobs
{
    /// <summary>
    /// US-GL03-01 / AC#5 — auto-creates the next Financial Year + its 13 periods for each company
    /// whose current latest year ends within the next 3 months. Idempotent: skips companies that
    /// already have a year covering or starting after the current year's end + 1 day.
    /// Runs daily at 02:00 (scheduled in BSOFT.Api/Program.cs).
    /// </summary>
    public class AutoCreateNextFinancialYearMasterJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AutoCreateNextFinancialYearMasterJob> _logger;

        public AutoCreateNextFinancialYearMasterJob(
            IServiceScopeFactory scopeFactory,
            ILogger<AutoCreateNextFinancialYearMasterJob> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        [Queue("finance-jobs")]
        [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task ProcessAsync(CancellationToken cancellationToken)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();

            var companyLookup = scope.ServiceProvider.GetRequiredService<ICompanyLookup>();
            var queryRepo     = scope.ServiceProvider.GetRequiredService<IFinancialYearMasterQueryRepository>();
            var commandRepo   = scope.ServiceProvider.GetRequiredService<IFinancialYearMasterCommandRepository>();

            var today         = DateOnly.FromDateTime(DateTime.UtcNow);
            var thresholdDate = today.AddMonths(3);
            var companies     = await companyLookup.GetAllCompanyAsync();

            // Resolve status MiscMaster ids once
            var fysOpenId = await queryRepo.GetMiscMasterIdByCodeAsync("FYS", "OPEN");
            var fpsOpenId = await queryRepo.GetMiscMasterIdByCodeAsync("FPS", "OPEN");
            if (fysOpenId <= 0 || fpsOpenId <= 0)
            {
                _logger.LogError(
                    "AutoCreateNextFinancialYearMasterJob: MiscMaster status rows (FYS/FPS) are not seeded — skipping run.");
                return;
            }

            foreach (var company in companies)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var latest = await queryRepo.GetLatestForCompanyAsync(company.CompanyId, cancellationToken);
                    if (latest is null || latest.EndDate > thresholdDate)
                        continue;

                    var nextStart = latest.EndDate.AddDays(1);
                    var nextEnd   = nextStart.AddMonths(12).AddDays(-1);
                    var nextCode  = ComputeNextFinancialYearCode(latest.FinancialYearCode);
                    if (string.IsNullOrWhiteSpace(nextCode))
                    {
                        _logger.LogError(
                            "AutoCreateNextFinancialYearMasterJob: Could not compute next code from '{Code}' for Company {Id}.",
                            latest.FinancialYearCode, company.CompanyId);
                        continue;
                    }

                    // Skip if a year with this code already exists (idempotent)
                    if (await queryRepo.AlreadyExistsByCodeAsync(nextCode, company.CompanyId))
                    {
                        _logger.LogInformation(
                            "AutoCreateNextFinancialYearMasterJob: Company {Id} already has {Code} — skipping.",
                            company.CompanyId, nextCode);
                        continue;
                    }

                    var year = new FinanceManagement.Domain.Entities.FinancialYearMaster
                    {
                        CompanyId         = company.CompanyId,
                        FinancialYearCode = nextCode,
                        StartDate         = nextStart,
                        EndDate           = nextEnd,
                        StatusId          = fysOpenId,
                        IsTransitionYear  = false,
                        IsActive          = Status.Active,
                        IsDeleted         = IsDelete.NotDeleted
                    };

                    var periods = GeneratePeriods(year, fpsOpenId);

                    await commandRepo.CreateAsync(year, periods, cancellationToken);
                    _logger.LogInformation(
                        "AutoCreateNextFinancialYearMasterJob: Auto-created {Code} ({Start}..{End}) for Company {Id}.",
                        nextCode, nextStart, nextEnd, company.CompanyId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "AutoCreateNextFinancialYearMasterJob: Failed for Company {Id}.", company.CompanyId);
                }
            }
        }

        private static string? ComputeNextFinancialYearCode(string? currentCode)
        {
            if (string.IsNullOrWhiteSpace(currentCode) || currentCode.Length != 7) return null;
            if (!int.TryParse(currentCode.Substring(0, 4), out var year)) return null;
            var nextStart = year + 1;
            var nextEnd   = (nextStart + 1) % 100;
            return $"{nextStart}-{nextEnd:D2}";
        }

        private static List<FinanceManagement.Domain.Entities.FinancialPeriodMaster> GeneratePeriods(
            FinanceManagement.Domain.Entities.FinancialYearMaster year, int fpsOpenId)
        {
            var periods = new List<FinanceManagement.Domain.Entities.FinancialPeriodMaster>();

            for (byte p = 1; p <= 12; p++)
            {
                var pStart = year.StartDate.AddMonths(p - 1);
                var pEnd   = pStart.AddMonths(1).AddDays(-1);
                periods.Add(new FinanceManagement.Domain.Entities.FinancialPeriodMaster
                {
                    CompanyId          = year.CompanyId,
                    PeriodNumber       = p,
                    PeriodName         = pStart.ToString("MMM-yyyy"),
                    StartDate          = pStart,
                    EndDate            = pEnd,
                    StatusId           = fpsOpenId,
                    IsAdjustmentPeriod = false,
                    IsActive           = Status.Active,
                    IsDeleted          = IsDelete.NotDeleted
                });
            }

            var p12 = periods[^1];
            periods.Add(new FinanceManagement.Domain.Entities.FinancialPeriodMaster
            {
                CompanyId          = year.CompanyId,
                PeriodNumber       = 13,
                PeriodName         = $"Adj-{year.FinancialYearCode}",
                StartDate          = p12.StartDate,
                EndDate            = p12.EndDate,
                StatusId           = fpsOpenId,
                IsAdjustmentPeriod = true,
                IsActive           = Status.Active,
                IsDeleted          = IsDelete.NotDeleted
            });

            return periods;
        }
    }
}
