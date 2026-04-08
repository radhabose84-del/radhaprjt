using AutoMapper;
using Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IEntity;
using UserManagement.Application.Entity.Queries.GetEntity;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Entity.Queries
{
    public sealed class GetEntityQueryHandlerTests
    {
        private readonly Mock<IEntityQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetEntityQueryHandler>> _mockLogger = new(MockBehavior.Loose);

        private GetEntityQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<UserManagement.Domain.Entities.Entity> { new() { Id = 1 } };
            var dtoList = new List<GetEntityDTO> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetAllEntityAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetEntityDTO>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetEntityQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NoResults_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllEntityAsync(1, 10, null))
                .ReturnsAsync((new List<UserManagement.Domain.Entities.Entity>(), 0));

            var result = await CreateSut().Handle(
                new GetEntityQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }
    }
}
