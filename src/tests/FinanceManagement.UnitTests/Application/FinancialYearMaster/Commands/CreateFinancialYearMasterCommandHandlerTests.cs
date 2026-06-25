using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Commands.CreateFinancialYearMaster;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.FinancialYearMaster.Commands
{
    public sealed class CreateFinancialYearMasterCommandHandlerTests
    {
        private readonly Mock<IFinancialYearMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IFinancialYearMasterQueryRepository>   _mockQueryRepo   = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp       = new(MockBehavior.Loose);
        private readonly Mock<IMediator>         _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper>           _mockMapper   = new(MockBehavior.Loose);

        private CreateFinancialYearMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockIp.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int fysOpenId = 100, int fpsOpenId = 200, int newId = 1, int? companyId = 1)
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(companyId);

            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("FYS", "OPEN")).ReturnsAsync(fysOpenId);
            _mockQueryRepo.Setup(r => r.GetMiscMasterIdByCodeAsync("FPS", "OPEN")).ReturnsAsync(fpsOpenId);

            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.FinancialYearMaster>(It.IsAny<CreateFinancialYearMasterCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.FinancialYearMaster());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.FinancialYearMaster>(),
                    It.IsAny<IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessWithNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(FinancialYearMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(42);
            result.Message.Should().Contain("created");
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsCompanyIdFromSession()
        {
            FinanceManagement.Domain.Entities.FinancialYearMaster? captured = null;
            SetupHappyPath(companyId: 7);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.FinancialYearMaster>(),
                    It.IsAny<IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<FinanceManagement.Domain.Entities.FinancialYearMaster, IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>, CancellationToken>(
                    (yr, _, _) => captured = yr)
                .ReturnsAsync(1);

            await CreateSut().Handle(FinancialYearMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            captured!.CompanyId.Should().Be(7);
        }

        [Fact]
        public async Task Handle_ValidCommand_AssignsFysOpenStatusIdToYear()
        {
            FinanceManagement.Domain.Entities.FinancialYearMaster? captured = null;
            SetupHappyPath(fysOpenId: 999);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.FinancialYearMaster>(),
                    It.IsAny<IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<FinanceManagement.Domain.Entities.FinancialYearMaster, IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>, CancellationToken>(
                    (yr, _, _) => captured = yr)
                .ReturnsAsync(1);

            await CreateSut().Handle(FinancialYearMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            captured!.StatusId.Should().Be(999);
        }

        [Fact]
        public async Task Handle_ValidCommand_Generates13Periods()
        {
            IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>? capturedPeriods = null;
            SetupHappyPath();

            _mockCommandRepo
                .Setup(r => r.CreateAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.FinancialYearMaster>(),
                    It.IsAny<IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<FinanceManagement.Domain.Entities.FinancialYearMaster, IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>, CancellationToken>(
                    (_, p, _) => capturedPeriods = p)
                .ReturnsAsync(1);

            await CreateSut().Handle(FinancialYearMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            capturedPeriods.Should().NotBeNull();
            capturedPeriods!.Count.Should().Be(13);
        }

        [Fact]
        public async Task Handle_ValidCommand_Period12And13ShareDates_IndianAccountingConvention()
        {
            IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>? captured = null;
            SetupHappyPath();

            _mockCommandRepo
                .Setup(r => r.CreateAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.FinancialYearMaster>(),
                    It.IsAny<IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<FinanceManagement.Domain.Entities.FinancialYearMaster, IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>, CancellationToken>(
                    (_, p, _) => captured = p)
                .ReturnsAsync(1);

            await CreateSut().Handle(FinancialYearMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            var p12 = captured!.Single(x => x.PeriodNumber == 12);
            var p13 = captured.Single(x => x.PeriodNumber == 13);

            p13.StartDate.Should().Be(p12.StartDate);
            p13.EndDate.Should().Be(p12.EndDate);
            p13.IsAdjustmentPeriod.Should().BeTrue();
            p12.IsAdjustmentPeriod.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_ValidCommand_AllPeriodsAssignedFpsOpenStatusId()
        {
            IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>? captured = null;
            SetupHappyPath(fpsOpenId: 555);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.FinancialYearMaster>(),
                    It.IsAny<IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<FinanceManagement.Domain.Entities.FinancialYearMaster, IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>, CancellationToken>(
                    (_, p, _) => captured = p)
                .ReturnsAsync(1);

            await CreateSut().Handle(FinancialYearMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            captured!.Should().OnlyContain(p => p.StatusId == 555);
        }

        [Fact]
        public async Task Handle_ValidCommand_Period1IsAprilOfStartYear()
        {
            IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>? captured = null;
            SetupHappyPath();

            _mockCommandRepo
                .Setup(r => r.CreateAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.FinancialYearMaster>(),
                    It.IsAny<IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>>(),
                    It.IsAny<CancellationToken>()))
                .Callback<FinanceManagement.Domain.Entities.FinancialYearMaster, IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>, CancellationToken>(
                    (_, p, _) => captured = p)
                .ReturnsAsync(1);

            await CreateSut().Handle(FinancialYearMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            var p1 = captured!.Single(x => x.PeriodNumber == 1);
            p1.StartDate.Should().Be(new DateOnly(2024, 4, 1));
            p1.EndDate.Should().Be(new DateOnly(2024, 4, 30));
            p1.PeriodName.Should().Be("Apr-2024");
        }

        [Fact]
        public async Task Handle_NoCompanyInSession_ThrowsExceptionRules()
        {
            SetupHappyPath(companyId: null);

            Func<Task> act = () => CreateSut().Handle(FinancialYearMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*No active company*");
        }

        [Fact]
        public async Task Handle_FysOpenStatusNotSeeded_ThrowsExceptionRules()
        {
            SetupHappyPath(fysOpenId: 0);

            Func<Task> act = () => CreateSut().Handle(FinancialYearMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*FYS / FPS*");
        }

        [Fact]
        public async Task Handle_FpsOpenStatusNotSeeded_ThrowsExceptionRules()
        {
            SetupHappyPath(fpsOpenId: 0);

            Func<Task> act = () => CreateSut().Handle(FinancialYearMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(FinancialYearMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "FINANCIAL_YEAR_MASTER_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnceWithBothYearAndPeriods()
        {
            SetupHappyPath();
            await CreateSut().Handle(FinancialYearMasterBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(
                    It.IsAny<FinanceManagement.Domain.Entities.FinancialYearMaster>(),
                    It.Is<IReadOnlyList<FinanceManagement.Domain.Entities.FinancialPeriodMaster>>(p => p.Count == 13),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
