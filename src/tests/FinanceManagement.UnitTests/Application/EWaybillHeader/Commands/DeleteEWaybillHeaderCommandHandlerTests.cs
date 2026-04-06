using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Commands.DeleteEWaybillHeader;

namespace FinanceManagement.UnitTests.Application.EWaybillHeader.Commands
{
    public sealed class DeleteEWaybillHeaderCommandHandlerTests
    {
        private readonly Mock<IEWaybillHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteEWaybillHeaderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeleteEWaybillHeaderCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_CallsSoftDeleteOnce()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(new DeleteEWaybillHeaderCommand(1), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(new DeleteEWaybillHeaderCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "EWAYBILL_HEADER_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
