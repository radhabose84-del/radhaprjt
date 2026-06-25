using BackgroundService.Infrastructure.Jobs;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Dto;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BackgroundService.UnitTests.Jobs
{
    /// <summary>
    /// US-GL03-01 / AC#5 — auto-create next FY when current ends within 3 months.
    /// </summary>
    public sealed class AutoCreateNextFinancialYearMasterJobTests
    {
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearMasterQueryRepository>   _mockQueryRepo   = new(MockBehavior.Loose);
        private readonly Mock<IFinancialYearMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);

        private IServiceScopeFactory BuildScopeFactory()
        {
            var services = new ServiceCollection();
            services.AddSingleton(_mockCompanyLookup.Object);
            services.AddSingleton(_mockQueryRepo.Object);
            services.AddSingleton(_mockCommandRepo.Object);
            return services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
        }

        private AutoCreateNextFinancialYearMasterJob CreateSut() =>
            new(BuildScopeFactory(), NullLogger<AutoCreateNextFinancialYearMasterJob>.Instance);

        [Fact]
        public async Task ProcessAsync_NoStatusSeeded_DoesNotCreate()
        {
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("FYS", "OPEN")).ReturnsAsync(0);
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("FPS", "OPEN")).ReturnsAsync(0);
            _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Co1" } });

            await CreateSut().ProcessAsync(CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(
                It.IsAny<FinanceManagement.Domain.Entities.FinancialYearMaster>(),
                It.IsAny<IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_NoLatestYear_Skips()
        {
            SeededStatusIds(100, 200);
            _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Co1" } });
            _mockQueryRepo.Setup(r => r.GetLatestForCompanyAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((FinancialYearMasterLookupDto?)null);

            await CreateSut().ProcessAsync(CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(
                It.IsAny<FinanceManagement.Domain.Entities.FinancialYearMaster>(),
                It.IsAny<IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_LatestEndsMoreThan3MonthsOut_Skips()
        {
            SeededStatusIds(100, 200);
            _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Co1" } });

            var farFuture = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(2);
            _mockQueryRepo.Setup(r => r.GetLatestForCompanyAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialYearMasterLookupDto
                {
                    Id = 1, CompanyId = 1, FinancialYearCode = "2024-25",
                    StartDate = farFuture.AddYears(-1).AddDays(1), EndDate = farFuture
                });

            await CreateSut().ProcessAsync(CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(
                It.IsAny<FinanceManagement.Domain.Entities.FinancialYearMaster>(),
                It.IsAny<IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task ProcessAsync_LatestEndsWithin3Months_CreatesNextYearWith13Periods()
        {
            SeededStatusIds(100, 200);
            _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Co1" } });

            _mockQueryRepo.Setup(r => r.GetLatestForCompanyAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialYearMasterLookupDto
                {
                    Id = 1, CompanyId = 1, FinancialYearCode = "2024-25",
                    StartDate = new DateOnly(2024, 4, 1),
                    EndDate   = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30)
                });
            _mockQueryRepo.Setup(r => r.AlreadyExistsByCodeAsync(It.IsAny<string>(), 1, It.IsAny<int?>())).ReturnsAsync(false);
            _mockCommandRepo.Setup(r => r.CreateAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.FinancialYearMaster>(),
                    It.IsAny<IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(99);

            await CreateSut().ProcessAsync(CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(
                It.IsAny<FinanceManagement.Domain.Entities.FinancialYearMaster>(),
                It.Is<IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>>(p => p.Count == 13),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ProcessAsync_NextCodeAlreadyExists_SkipsIdempotently()
        {
            SeededStatusIds(100, 200);
            _mockCompanyLookup.Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto> { new() { CompanyId = 1, CompanyName = "Co1" } });

            _mockQueryRepo.Setup(r => r.GetLatestForCompanyAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FinancialYearMasterLookupDto
                {
                    Id = 1, CompanyId = 1, FinancialYearCode = "2024-25",
                    StartDate = new DateOnly(2024, 4, 1),
                    EndDate   = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30)
                });
            _mockQueryRepo.Setup(r => r.AlreadyExistsByCodeAsync("2025-26", 1, It.IsAny<int?>())).ReturnsAsync(true);

            await CreateSut().ProcessAsync(CancellationToken.None);

            _mockCommandRepo.Verify(r => r.CreateAsync(
                It.IsAny<FinanceManagement.Domain.Entities.FinancialYearMaster>(),
                It.IsAny<IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        private void SeededStatusIds(int fys, int fps)
        {
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("FYS", "OPEN")).ReturnsAsync(fys);
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("FPS", "OPEN")).ReturnsAsync(fps);
        }
    }
}
