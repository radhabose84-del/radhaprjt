using AutoMapper;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IEntity;
using UserManagement.Application.Entity.Queries.GetEntityLastCode;

namespace UserManagement.UnitTests.Application.Entity.Queries
{
    public sealed class GetEntityLastCodeQueryHandlerTests
    {
        private readonly Mock<IEntityQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetEntityLastCodeQueryHandler>> _mockLogger = new();

        private GetEntityLastCodeQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ReturnsCode()
        {
            _mockRepo.Setup(r => r.GenerateEntityCodeAsync()).ReturnsAsync("E001");

            var result = await CreateSut().Handle(new GetEntityLastCodeQuery(), CancellationToken.None);
            result.Data.Should().Be("E001");
            result.IsSuccess.Should().BeTrue();
        }
    }
}
