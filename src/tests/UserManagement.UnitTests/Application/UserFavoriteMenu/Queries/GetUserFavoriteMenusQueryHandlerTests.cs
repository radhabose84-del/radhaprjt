using Contracts.Interfaces;
using MediatR;
using UserManagement.Application.Common.Interfaces.IUserFavoriteMenu;
using UserManagement.Application.UserFavoriteMenu.Dto;
using UserManagement.Application.UserFavoriteMenu.Queries.GetUserFavoriteMenus;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.UserFavoriteMenu.Queries
{
    public sealed class GetUserFavoriteMenusQueryHandlerTests
    {
        private readonly Mock<IUserFavoriteMenuQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Strict);

        private GetUserFavoriteMenusQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockIpService.Object);

        [Fact]
        public async Task Handle_WithFavorites_ReturnsList()
        {
            // Arrange
            var dtos = new List<UserFavoriteMenuDto>
            {
                new() { MenuId = 1, MenuName = "Dashboard", MenuUrl = "/dashboard" },
                new() { MenuId = 2, MenuName = "Reports", MenuUrl = "/reports" }
            };

            _mockIpService
                .Setup(s => s.GetUserId())
                .Returns(1);

            _mockQueryRepo
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(new GetUserFavoriteMenusQuery(), CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_NoFavorites_ReturnsEmptyList()
        {
            // Arrange
            _mockIpService
                .Setup(s => s.GetUserId())
                .Returns(5);

            _mockQueryRepo
                .Setup(r => r.GetByUserIdAsync(5))
                .ReturnsAsync(new List<UserFavoriteMenuDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(new GetUserFavoriteMenusQuery(), CancellationToken.None);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithFavorites_PublishesAuditEvent()
        {
            // Arrange
            var dtos = new List<UserFavoriteMenuDto>
            {
                new() { MenuId = 1, MenuName = "Dashboard" }
            };

            _mockIpService
                .Setup(s => s.GetUserId())
                .Returns(1);

            _mockQueryRepo
                .Setup(r => r.GetByUserIdAsync(1))
                .ReturnsAsync(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(new GetUserFavoriteMenusQuery(), CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetUserFavoriteMenus" &&
                        e.Module == "UserFavoriteMenu"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_UsesUserIdFromIpService()
        {
            // Arrange
            var dtos = new List<UserFavoriteMenuDto>();

            _mockIpService
                .Setup(s => s.GetUserId())
                .Returns(99);

            _mockQueryRepo
                .Setup(r => r.GetByUserIdAsync(99))
                .ReturnsAsync(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(new GetUserFavoriteMenusQuery(), CancellationToken.None);

            // Assert
            _mockQueryRepo.Verify(r => r.GetByUserIdAsync(99), Times.Once);
        }
    }
}
