using ProductionManagement.Application.Common.Interfaces.IPackType;
using ProductionManagement.Application.PackType.Queries.GetPackTypeAutoComplete;
using AppDto = Contracts.Dtos.Lookups.Production.PackTypeLookupDto;

namespace ProductionManagement.UnitTests.Application.PackType.Queries
{
    public sealed class GetPackTypeAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IPackTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPackTypeAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var lookup = new List<AppDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IReadOnlyList<AppDto>>(lookup));
            _mockMapper.Setup(m => m.Map<List<AppDto>>(It.IsAny<object>())).Returns(lookup);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetPackTypeAutoCompleteQuery("test"), CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NullTerm_ReturnsEmpty()
        {
            var lookup = new List<AppDto>();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult<IReadOnlyList<AppDto>>(lookup));
            _mockMapper.Setup(m => m.Map<List<AppDto>>(It.IsAny<object>())).Returns(lookup);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetPackTypeAutoCompleteQuery(null!), CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
