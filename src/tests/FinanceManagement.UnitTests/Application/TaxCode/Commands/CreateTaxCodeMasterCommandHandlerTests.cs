using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Commands.CreateTaxCodeMaster;

namespace FinanceManagement.UnitTests.Application.TaxCode.Commands
{
    public sealed class CreateTaxCodeMasterCommandHandlerTests
    {
        private readonly Mock<ITaxCodeCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        public CreateTaxCodeMasterCommandHandlerTests()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
        }

        private CreateTaxCodeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockIp.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateTaxCodeMasterCommand ValidCommand() =>
            new()
            {
                TaxCode = "GST-OUT-5",
                TaxName = "GST Output 5%",
                TaxTypeId = 10,
                TaxComponentId = 20,
                DirectionId = 30,
                RatePercent = 5.0m,
                EffectiveFrom = new DateOnly(2026, 6, 16)
            };

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.TaxCodeMaster>(It.IsAny<CreateTaxCodeMasterCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.TaxCodeMaster());
            _mockCommandRepo
                .Setup(r => r.CreateTaxCodeAsync(It.IsAny<FinanceManagement.Domain.Entities.TaxCodeMaster>()))
                .ReturnsAsync(newId);
            _mockCommandRepo
                .Setup(r => r.CreateRateVersionAsync(It.IsAny<FinanceManagement.Domain.Entities.TaxCodeRateVersion>()))
                .ReturnsAsync(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
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
        public async Task Handle_ValidCommand_SetsCompanyIdFromToken()
        {
            FinanceManagement.Domain.Entities.TaxCodeMaster? captured = null;
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.TaxCodeMaster>(It.IsAny<CreateTaxCodeMasterCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.TaxCodeMaster());
            _mockCommandRepo
                .Setup(r => r.CreateTaxCodeAsync(It.IsAny<FinanceManagement.Domain.Entities.TaxCodeMaster>()))
                .Callback<FinanceManagement.Domain.Entities.TaxCodeMaster>(e => captured = e)
                .ReturnsAsync(7);
            _mockCommandRepo
                .Setup(r => r.CreateRateVersionAsync(It.IsAny<FinanceManagement.Domain.Entities.TaxCodeRateVersion>()))
                .ReturnsAsync(1);
            _mockIp.Setup(x => x.GetCompanyId()).Returns(99);

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            captured!.CompanyId.Should().Be(99);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesInitialRateVersion()
        {
            SetupHappyPath(newId: 7);
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(
                r => r.CreateRateVersionAsync(It.Is<FinanceManagement.Domain.Entities.TaxCodeRateVersion>(
                    v => v.TaxCodeId == 7 && v.RatePercent == 5.0m)),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create" && e.ActionCode == "TAX_CODE_MASTER_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
