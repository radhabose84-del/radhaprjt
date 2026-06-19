using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Commands.DeleteVoucherType;

namespace FinanceManagement.UnitTests.Application.VoucherType.Commands
{
    public sealed class DeleteVoucherTypeCommandHandlerTests
    {
        private readonly Mock<IVoucherTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteVoucherTypeCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeleteVoucherTypeCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsSoftDeleteOnce()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Handle(new DeleteVoucherTypeCommand(5), CancellationToken.None);

            _mockCommandRepo.Verify(r => r.SoftDeleteAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            await CreateSut().Handle(new DeleteVoucherTypeCommand(7), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "VOUCHER_TYPE_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
