using MediatR;
using LogisticsManagement.Application.Common.Interfaces.IMiscTypeMaster;
using LogisticsManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster;
using LogisticsManagement.Domain.Events;

namespace LogisticsManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public sealed class DeleteMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteMiscTypeMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsTrue()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeleteMiscTypeMasterCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(new DeleteMiscTypeMasterCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "MISC_TYPE_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ExistingId_CallsSoftDeleteOnce()
        {
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(new DeleteMiscTypeMasterCommand(1), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_AlwaysReturnsTrue_EvenWhenSoftDeleteReturnsFalse()
        {
            // Note: DeleteMiscTypeMasterCommandHandler does NOT check the result of SoftDeleteAsync
            // It always publishes audit and returns true
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateSut().Handle(new DeleteMiscTypeMasterCommand(99), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_AlwaysPublishesAuditEvent()
        {
            // Handler always publishes audit event regardless of SoftDeleteAsync result
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            await CreateSut().Handle(new DeleteMiscTypeMasterCommand(99), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
