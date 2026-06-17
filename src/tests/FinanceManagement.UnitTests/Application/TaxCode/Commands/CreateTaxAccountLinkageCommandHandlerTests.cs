using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Commands.CreateTaxAccountLinkage;

namespace FinanceManagement.UnitTests.Application.TaxCode.Commands
{
    public sealed class CreateTaxAccountLinkageCommandHandlerTests
    {
        private readonly Mock<ITaxCodeCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ITaxCodeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        public CreateTaxAccountLinkageCommandHandlerTests()
        {
            _mockIp.Setup(x => x.GetCompanyId()).Returns(1);
        }

        private CreateTaxAccountLinkageCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockIp.Object, _mockMediator.Object, _mockMapper.Object);

        private static CreateTaxAccountLinkageCommand ValidCommand() =>
            new() { TaxCodeId = 14, GlAccountId = 412, ControlAccountId = 30, EffectiveFrom = new DateOnly(2026, 6, 16) };

        private void SetupHappyPath(int newId = 1, int approvedStatusId = 20)
        {
            _mockQueryRepo.Setup(r => r.GetMiscIdAsync("ApprovalStatus", "APPROVED")).ReturnsAsync(approvedStatusId);
            _mockMapper
                .Setup(m => m.Map<FinanceManagement.Domain.Entities.TaxAccountLinkage>(It.IsAny<CreateTaxAccountLinkageCommand>()))
                .Returns(new FinanceManagement.Domain.Entities.TaxAccountLinkage());
            _mockCommandRepo
                .Setup(r => r.CreateLinkageAsync(It.IsAny<FinanceManagement.Domain.Entities.TaxAccountLinkage>()))
                .ReturnsAsync(newId);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath(newId: 9);
            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(9);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockCommandRepo.Verify(r => r.CreateLinkageAsync(It.IsAny<FinanceManagement.Domain.Entities.TaxAccountLinkage>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(ValidCommand(), CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "TAX_ACCOUNT_LINKAGE_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
