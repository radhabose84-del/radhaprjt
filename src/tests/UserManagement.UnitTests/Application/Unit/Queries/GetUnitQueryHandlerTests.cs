using AutoMapper;
using Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Unit.Queries
{
    public sealed class GetUnitQueryHandlerTests
    {
        private readonly Mock<IUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetUnitQueryHandler>> _mockLogger = new(MockBehavior.Loose);

        private GetUnitQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            var entities = new List<UserManagement.Domain.Entities.Unit> { new() { Id = 1 } };
            var dtoList = new List<GetUnitsDTO> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetAllUnitsAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetUnitsDTO>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUnitQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NullResult_ThrowsNullReferenceException()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllUnitsAsync(1, 10, null))
                .ReturnsAsync(((List<UserManagement.Domain.Entities.Unit>?)null!, 0));

            // The handler accesses units.Count in the logging statement before the
            // null check completes, causing a NullReferenceException.
            Func<Task> act = async () => await CreateSut().Handle(
                new GetUnitQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            await act.Should().ThrowAsync<NullReferenceException>();
        }
    }
}
