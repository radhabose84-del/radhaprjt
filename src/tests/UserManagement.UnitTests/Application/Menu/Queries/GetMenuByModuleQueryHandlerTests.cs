using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMenu;
using UserManagement.Application.Menu.Queries.GetMenuByModule;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Menu.Queries
{
    public sealed class GetMenuByModuleQueryHandlerTests
    {
        private readonly Mock<IMenuQuery> _mockMenuQuery = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMenuByModuleQueryHandler CreateSut() =>
            new(_mockMenuQuery.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMenuList()
        {
            var entities = new List<UserManagement.Domain.Entities.Menu> { new() { Id = 1 } };
            var dtoList = new List<MenuDTO> { new() { Id = 1 } };

            _mockMenuQuery
                .Setup(r => r.GetParentMenus(It.IsAny<List<int>>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<MenuDTO>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMenuByModuleQuery { ModuleId = new List<int> { 1 } }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var entities = new List<UserManagement.Domain.Entities.Menu>();

            _mockMenuQuery
                .Setup(r => r.GetParentMenus(It.IsAny<List<int>>()))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<MenuDTO>>(entities))
                .Returns(new List<MenuDTO>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetMenuByModuleQuery { ModuleId = new List<int> { 1 } }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
