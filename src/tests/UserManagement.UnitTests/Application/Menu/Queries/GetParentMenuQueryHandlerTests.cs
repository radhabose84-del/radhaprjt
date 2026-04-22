using AutoMapper;
using UserManagement.Application.Common.Interfaces.IMenu;
using UserManagement.Application.Menu.Queries.GetParentMenu;

namespace UserManagement.UnitTests.Application.Menu.Queries
{
    public sealed class GetParentMenuQueryHandlerTests
    {
        private readonly Mock<IMenuQuery> _mockMenuQuery = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetParentMenuQueryHandler CreateSut() =>
            new(_mockMenuQuery.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsParentMenuList()
        {
            var entities = new List<UserManagement.Domain.Entities.Menu> { new() { Id = 1 } };
            var dtoList = new List<ParentMenuDto> { new() { Id = 1 } };

            _mockMenuQuery
                .Setup(r => r.GetParentMenuAutoComplete("test", null))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<ParentMenuDto>>(entities))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetParentMenuQuery { SearchPattern = "test" }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            var entities = new List<UserManagement.Domain.Entities.Menu>();

            _mockMenuQuery
                .Setup(r => r.GetParentMenuAutoComplete("none", null))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<ParentMenuDto>>(entities))
                .Returns(new List<ParentMenuDto>());

            var result = await CreateSut().Handle(
                new GetParentMenuQuery { SearchPattern = "none" }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
