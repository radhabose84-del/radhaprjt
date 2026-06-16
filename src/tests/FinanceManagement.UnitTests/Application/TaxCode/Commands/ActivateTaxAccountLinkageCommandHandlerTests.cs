using FinanceManagement.Application.Common.Interfaces.ITaxCode;
using FinanceManagement.Application.TaxCode.Commands.ActivateTaxAccountLinkage;

namespace FinanceManagement.UnitTests.Application.TaxCode.Commands
{
    public sealed class ActivateTaxAccountLinkageCommandHandlerTests
    {
        private readonly Mock<ITaxCodeCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ITaxCodeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private ActivateTaxAccountLinkageCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object);

        private void SetupHappyPath(int id, int approvedStatusId = 20)
        {
            _mockQueryRepo.Setup(r => r.GetMiscIdAsync("ApprovalStatus", "APPROVED")).ReturnsAsync(approvedStatusId);
            _mockCommandRepo.Setup(r => r.ActivateLinkageAsync(id, approvedStatusId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        }

        [Fact]
        public async Task Handle_ValidId_ReturnsSuccess()
        {
            SetupHappyPath(3);

            var result = await CreateSut().Handle(new ActivateTaxAccountLinkageCommand(3), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(3);
        }

        [Fact]
        public async Task Handle_ValidId_CallsActivateWithApprovedStatus()
        {
            SetupHappyPath(3, approvedStatusId: 20);

            await CreateSut().Handle(new ActivateTaxAccountLinkageCommand(3), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.ActivateLinkageAsync(3, 20, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            SetupHappyPath(3);

            await CreateSut().Handle(new ActivateTaxAccountLinkageCommand(3), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "TAX_ACCOUNT_LINKAGE_ACTIVATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
