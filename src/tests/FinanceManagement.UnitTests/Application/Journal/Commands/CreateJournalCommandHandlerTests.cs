using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Journal.Commands.CreateJournal;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.Journal.Commands
{
    public sealed class CreateJournalCommandHandlerTests
    {
        private readonly Mock<IJournalCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IJournalQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateJournalCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockIp.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockIp.Setup(s => s.GetUnitId()).Returns(1);
            _mockQueryRepo.Setup(r => r.GetOpenPeriodByDateAsync(1, It.IsAny<DateOnly>()))
                .ReturnsAsync(((int PeriodId, int FinancialYearId)?)(4, 3));
            _mockQueryRepo.Setup(r => r.GetStatusIdAsync("DRAFT")).ReturnsAsync(101);
            _mockQueryRepo.Setup(r => r.GetSourceIdAsync("MANUAL")).ReturnsAsync(111);
            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.JournalHeader>(It.IsAny<CreateJournalCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.JournalHeader());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.JournalHeader>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(JournalBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("draft");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 77);
            var result = await CreateSut().Handle(JournalBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Data.Should().Be(77);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsHeaderTotalsAndContextFromSessionAndPeriod()
        {
            FinanceManagement.Domain.Entities.JournalHeader? captured = null;
            _mockIp.Setup(s => s.GetCompanyId()).Returns(9);
            _mockIp.Setup(s => s.GetUnitId()).Returns(5);
            _mockQueryRepo.Setup(r => r.GetOpenPeriodByDateAsync(9, It.IsAny<DateOnly>()))
                .ReturnsAsync(((int, int)?)(4, 3));
            _mockQueryRepo.Setup(r => r.GetStatusIdAsync("DRAFT")).ReturnsAsync(101);
            _mockQueryRepo.Setup(r => r.GetSourceIdAsync("MANUAL")).ReturnsAsync(111);
            _mockMapper.Setup(m => m.Map<FinanceManagement.Domain.Entities.JournalHeader>(It.IsAny<CreateJournalCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.JournalHeader());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.JournalHeader>()))
                .Callback<FinanceManagement.Domain.Entities.JournalHeader>(e => captured = e)
                .ReturnsAsync(1);

            await CreateSut().Handle(JournalBuilders.ValidCreateCommand(), CancellationToken.None);

            captured!.CompanyId.Should().Be(9);
            captured.UnitId.Should().Be(5);
            captured.AccountingPeriodId.Should().Be(4);
            captured.FinancialYearId.Should().Be(3);
            captured.StatusId.Should().Be(101);
            captured.SourceId.Should().Be(111);
            captured.TotalDr.Should().Be(1000m);
            captured.TotalCr.Should().Be(1000m);
            captured.Details.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_NoCompanyInSession_Throws()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns((int?)null);

            var act = async () => await CreateSut().Handle(JournalBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_NoOpenPeriod_Throws()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.GetOpenPeriodByDateAsync(1, It.IsAny<DateOnly>()))
                .ReturnsAsync(((int, int)?)null);

            var act = async () => await CreateSut().Handle(JournalBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(JournalBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create" && e.ActionCode == "JOURNAL_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
