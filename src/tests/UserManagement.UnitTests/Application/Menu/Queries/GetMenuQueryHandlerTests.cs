using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IMenu;
using UserManagement.Application.Menu.Queries.GetMenu;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Menu.Queries
{
    public sealed class GetMenuQueryHandlerTests
    {
        private readonly Mock<IMenuQuery> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMenuQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var data = new List<MenuDto> { new() { Id = 1 } };
            _mockRepo.Setup(r => r.GetAllMenuAsync(1, 10, null)).ReturnsAsync((data, 1));
            _mockMapper.Setup(m => m.Map<List<MenuDto>>(data)).Returns(data);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetMenuQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }
    }
}
