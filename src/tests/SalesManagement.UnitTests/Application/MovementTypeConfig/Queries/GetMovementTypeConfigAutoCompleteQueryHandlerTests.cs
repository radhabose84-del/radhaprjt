using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Dto;
using SalesManagement.Application.MovementTypeConfig.Queries.GetMovementTypeConfigAutoComplete;

namespace SalesManagement.UnitTests.Application.MovementTypeConfig.Queries
{
    public class GetMovementTypeConfigAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMovementTypeConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetMovementTypeConfigAutoCompleteQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<MovementTypeConfigLookupDto>>(It.IsAny<object>()))
                .Returns<object>(o => o is IEnumerable<MovementTypeConfigLookupDto> e ? e.ToList() : new List<MovementTypeConfigLookupDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetMovementTypeConfigAutoCompleteQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_WithTerm_ReturnsLookupList()
        {
            var lookupList = new List<MovementTypeConfigLookupDto>
            {
                new() { Id = 1, MovementCode = "M001", MovementDescription = "Movement A" },
                new() { Id = 2, MovementCode = "M002", MovementDescription = "Movement B" }
            } as IReadOnlyList<MovementTypeConfigLookupDto>;

            _mockQueryRepo.Setup(r => r.AutocompleteAsync("move", It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            var result = await CreateSut().Handle(
                new GetMovementTypeConfigAutoCompleteQuery("move"), CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_WithTerm_CallsAutocompleteAsync_Once()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MovementTypeConfigLookupDto>() as IReadOnlyList<MovementTypeConfigLookupDto>);

            await CreateSut().Handle(
                new GetMovementTypeConfigAutoCompleteQuery("test"), CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.AutocompleteAsync("test", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsEmptyList()
        {
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(string.Empty, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<MovementTypeConfigLookupDto>() as IReadOnlyList<MovementTypeConfigLookupDto>);

            var result = await CreateSut().Handle(
                new GetMovementTypeConfigAutoCompleteQuery(string.Empty), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
