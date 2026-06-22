using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalImport;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Application.JournalMaster.JournalImport.Commands.ImportJournals;
using FinanceManagement.Domain.Entities;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.JournalImport
{
    public sealed class ImportJournalsCommandHandlerTests
    {
        private readonly Mock<IJournalImportCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IJournalImportQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private ImportJournalsCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockIp.Object, _mockMediator.Object);

        private void SetupLookups(bool accountsExist = true)
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockIp.Setup(s => s.GetUserId()).Returns(1);
            _mockQueryRepo.Setup(r => r.GetSourceIdAsync("IMPORT")).ReturnsAsync(116);
            _mockQueryRepo.Setup(r => r.GetStatusIdAsync("DRAFT")).ReturnsAsync(101);
            _mockQueryRepo.Setup(r => r.GetBatchStatusIdAsync("COMMITTED")).ReturnsAsync(163);
            _mockQueryRepo.Setup(r => r.GetBatchStatusIdAsync("FAILED")).ReturnsAsync(164);
            _mockQueryRepo.Setup(r => r.GetExistingGlAccountIdsAsync(It.IsAny<IEnumerable<int>>(), 1))
                .ReturnsAsync(accountsExist ? new[] { 5400101, 2200105 } : Array.Empty<int>());
            _mockQueryRepo.Setup(r => r.GetExistingVoucherTypeIdsAsync(It.IsAny<IEnumerable<int>>(), 1)).ReturnsAsync(new[] { 1 });
            _mockQueryRepo.Setup(r => r.GetExistingCurrencyIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(new[] { 1 });
            _mockQueryRepo.Setup(r => r.GetExistingCostCentreIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(new[] { 1 });
            _mockQueryRepo.Setup(r => r.GetExistingProfitCentreIdsAsync(It.IsAny<IEnumerable<int>>())).ReturnsAsync(new[] { 1 });
            _mockQueryRepo.Setup(r => r.GetOpenPeriodByDateAsync(1, It.IsAny<DateOnly>())).ReturnsAsync(((int, int)?)(4, 3));
        }

        [Fact]
        public async Task Handle_ValidRows_CommitsDrafts()
        {
            SetupLookups();
            _mockCommandRepo
                .Setup(r => r.CommitAsync(It.IsAny<JournalImportBatch>(), It.IsAny<List<JournalHeader>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((50, new List<int> { 101 }));

            var result = await CreateSut().Handle(JournalImportBuilders.ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.Committed.Should().BeTrue();
            result.Data.CreatedJournalIds.Should().ContainSingle();
            result.Data.ErrorRows.Should().Be(0);
        }

        [Fact]
        public async Task Handle_ValidRows_BuildsOneDraftPerGroup()
        {
            SetupLookups();
            List<JournalHeader>? captured = null;
            _mockCommandRepo
                .Setup(r => r.CommitAsync(It.IsAny<JournalImportBatch>(), It.IsAny<List<JournalHeader>>(), It.IsAny<CancellationToken>()))
                .Callback<JournalImportBatch, List<JournalHeader>, CancellationToken>((_, d, _) => captured = d)
                .ReturnsAsync((50, new List<int> { 101 }));

            await CreateSut().Handle(JournalImportBuilders.ValidCommand(), CancellationToken.None);

            captured.Should().ContainSingle();
            captured![0].Details.Should().HaveCount(2);
            captured[0].TotalDr.Should().Be(1000m);
            captured[0].SourceId.Should().Be(116);   // IMPORT
        }

        [Fact]
        public async Task Handle_InvalidAccount_RecordsErrors_NoCommit()
        {
            SetupLookups(accountsExist: false);
            _mockCommandRepo
                .Setup(r => r.SaveFailedBatchAsync(It.IsAny<JournalImportBatch>(), It.IsAny<IEnumerable<JournalImportError>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(60);

            var result = await CreateSut().Handle(JournalImportBuilders.ValidCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Data!.Committed.Should().BeFalse();
            result.Data.Errors.Should().NotBeEmpty();
            _mockCommandRepo.Verify(r => r.CommitAsync(It.IsAny<JournalImportBatch>(), It.IsAny<List<JournalHeader>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_UnbalancedGroup_RecordsError_NoCommit()
        {
            SetupLookups();
            _mockCommandRepo
                .Setup(r => r.SaveFailedBatchAsync(It.IsAny<JournalImportBatch>(), It.IsAny<IEnumerable<JournalImportError>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(61);

            var rows = JournalImportBuilders.ValidRows();
            rows[1].CrAmount = 900m;   // group no longer balances

            var result = await CreateSut().Handle(JournalImportBuilders.ValidCommand(rows: rows), CancellationToken.None);

            result.Data!.Committed.Should().BeFalse();
            result.Data.Errors.Should().Contain(e => e.Message!.Contains("out of balance"));
        }

        [Fact]
        public async Task Handle_EmptyRows_Fails()
        {
            SetupLookups();
            _mockCommandRepo
                .Setup(r => r.SaveFailedBatchAsync(It.IsAny<JournalImportBatch>(), It.IsAny<IEnumerable<JournalImportError>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(62);

            var result = await CreateSut().Handle(JournalImportBuilders.ValidCommand(rows: new List<JournalImportRowInputDto>()), CancellationToken.None);

            result.Data!.Committed.Should().BeFalse();
        }
    }
}
