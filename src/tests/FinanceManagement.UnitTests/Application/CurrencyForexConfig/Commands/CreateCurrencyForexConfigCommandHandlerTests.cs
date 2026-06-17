using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Commands.CreateCurrencyForexConfig;

namespace FinanceManagement.UnitTests.Application.CurrencyForexConfig.Commands
{
    public sealed class CreateCurrencyForexConfigCommandHandlerTests
    {
        private readonly Mock<ICurrencyForexConfigCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateCurrencyForexConfigCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockIp.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateCurrencyForexConfigCommand ValidCommand() =>
            new() { CurrencyTypeCode = "FOREX", CurrencyTypeName = "Forex" };

        private void SetupHappyPath(int companyId = 1, int newId = 1)
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(companyId);
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.CurrencyForexConfig>(It.IsAny<CreateCurrencyForexConfigCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.CurrencyForexConfig());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.CurrencyForexConfig>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("created successfully");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 42);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsCompanyIdFromSession()
        {
            FinanceManagement.Domain.Entities.CurrencyForexConfig? captured = null;
            _mockIp.Setup(s => s.GetCompanyId()).Returns(9);
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.CurrencyForexConfig>(It.IsAny<CreateCurrencyForexConfigCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.CurrencyForexConfig());
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<FinanceManagement.Domain.Entities.CurrencyForexConfig>()))
                .Callback<FinanceManagement.Domain.Entities.CurrencyForexConfig>(e => captured = e)
                .ReturnsAsync(1);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            captured!.CompanyId.Should().Be(9);
        }

        [Fact]
        public async Task Handle_NoActiveCompany_Throws()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns((int?)null);

            Func<Task> act = async () => await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "CURRENCY_FOREX_CONFIG_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
