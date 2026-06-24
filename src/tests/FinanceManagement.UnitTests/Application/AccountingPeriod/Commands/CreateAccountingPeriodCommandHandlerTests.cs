using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IAccountingPeriod;
using FinanceManagement.Application.JournalMaster.AccountingPeriod.Commands.CreateAccountingPeriod;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Application.AccountingPeriod.Commands
{
    public sealed class CreateAccountingPeriodCommandHandlerTests
    {
        private readonly Mock<IAccountingPeriodCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateAccountingPeriodCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockIp.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.AccountingPeriod>(It.IsAny<CreateAccountingPeriodCommand>()))
                .Returns(AccountingPeriodBuilders.ValidEntity());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.AccountingPeriod>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(AccountingPeriodBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("created successfully");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(AccountingPeriodBuilders.ValidCreateCommand(), CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsCompanyIdFromSession()
        {
            FinanceManagement.Domain.Entities.AccountingPeriod? captured = null;
            _mockIp.Setup(s => s.GetCompanyId()).Returns(7);
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.AccountingPeriod>(It.IsAny<CreateAccountingPeriodCommand>()))
                .Returns(AccountingPeriodBuilders.ValidEntity());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.AccountingPeriod>()))
                .Callback<FinanceManagement.Domain.Entities.AccountingPeriod>(e => captured = e)
                .ReturnsAsync(1);

            await CreateSut().Handle(AccountingPeriodBuilders.ValidCreateCommand(), CancellationToken.None);

            captured!.CompanyId.Should().Be(7);
        }

        [Fact]
        public async Task Handle_NoCompanyInSession_Throws()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns((int?)null);

            var act = async () => await CreateSut().Handle(AccountingPeriodBuilders.ValidCreateCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(AccountingPeriodBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "ACCOUNTING_PERIOD_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
