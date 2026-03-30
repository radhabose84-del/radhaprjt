using Contracts.Interfaces;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserFavoriteMenu;
using UserManagement.Application.UserFavoriteMenu.Commands.RemoveUserFavoriteMenu;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.UserFavoriteMenu.Commands
{
    public sealed class RemoveUserFavoriteMenuCommandHandlerTests
    {
        private readonly Mock<IUserFavoriteMenuCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Strict);

        private RemoveUserFavoriteMenuCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockIpService.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            // Arrange
            var command = new RemoveUserFavoriteMenuCommand(MenuId: 42);

            _mockIpService
                .Setup(s => s.GetUserId())
                .Returns(1);

            _mockCommandRepo
                .Setup(r => r.HardDeleteAsync(1, 42, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DeleteFails_ThrowsException()
        {
            // Arrange
            var command = new RemoveUserFavoriteMenuCommand(MenuId: 99);

            _mockIpService
                .Setup(s => s.GetUserId())
                .Returns(1);

            _mockCommandRepo
                .Setup(r => r.HardDeleteAsync(1, 99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("*remove menu from favorites*");
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            // Arrange
            var command = new RemoveUserFavoriteMenuCommand(MenuId: 42);

            _mockIpService
                .Setup(s => s.GetUserId())
                .Returns(3);

            _mockCommandRepo
                .Setup(r => r.HardDeleteAsync(3, 42, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Delete" &&
                        e.ActionCode == "USERFAVORITEMENU_DELETE" &&
                        e.Module == "UserFavoriteMenu"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DeleteFails_DoesNotPublishAuditEvent()
        {
            // Arrange
            var command = new RemoveUserFavoriteMenuCommand(MenuId: 99);

            _mockIpService
                .Setup(s => s.GetUserId())
                .Returns(1);

            _mockCommandRepo
                .Setup(r => r.HardDeleteAsync(1, 99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var sut = CreateSut();

            // Act
            try { await sut.Handle(command, CancellationToken.None); }
            catch { /* expected */ }

            // Assert
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
