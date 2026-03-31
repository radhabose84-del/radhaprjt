using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserFavoriteMenu;
using UserManagement.Application.UserFavoriteMenu.Commands.AddUserFavoriteMenu;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.UserFavoriteMenu.Commands
{
    public sealed class AddUserFavoriteMenuCommandHandlerTests
    {
        private readonly Mock<IUserFavoriteMenuCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Strict);

        private AddUserFavoriteMenuCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockIpService.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            // Arrange
            var command = new AddUserFavoriteMenuCommand { MenuId = 42 };
            var entity = new UserManagement.Domain.Entities.UserFavoriteMenu { MenuId = 42, UserId = 1 };

            _mockIpService
                .Setup(s => s.GetUserId())
                .Returns(1);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserFavoriteMenu>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(10);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(10);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsUserIdFromIpService()
        {
            // Arrange
            var command = new AddUserFavoriteMenuCommand { MenuId = 42 };
            var entity = new UserManagement.Domain.Entities.UserFavoriteMenu { MenuId = 42 };
            UserManagement.Domain.Entities.UserFavoriteMenu? capturedEntity = null;

            _mockIpService
                .Setup(s => s.GetUserId())
                .Returns(7);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserFavoriteMenu>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.UserFavoriteMenu>()))
                .Callback<UserManagement.Domain.Entities.UserFavoriteMenu>(e => capturedEntity = e)
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            capturedEntity!.UserId.Should().Be(7);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            // Arrange
            var command = new AddUserFavoriteMenuCommand { MenuId = 42 };
            var entity = new UserManagement.Domain.Entities.UserFavoriteMenu { MenuId = 42, UserId = 1 };

            _mockIpService
                .Setup(s => s.GetUserId())
                .Returns(1);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserFavoriteMenu>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(10);

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
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "USERFAVORITEMENU_CREATE" &&
                        e.Module == "UserFavoriteMenu"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            // Arrange
            var command = new AddUserFavoriteMenuCommand { MenuId = 5 };
            var entity = new UserManagement.Domain.Entities.UserFavoriteMenu { MenuId = 5, UserId = 2 };

            _mockIpService
                .Setup(s => s.GetUserId())
                .Returns(2);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.UserFavoriteMenu>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.UserFavoriteMenu>()),
                Times.Once);
        }
    }
}
