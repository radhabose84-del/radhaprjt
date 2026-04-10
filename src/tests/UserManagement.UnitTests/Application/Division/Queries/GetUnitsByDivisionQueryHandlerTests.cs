using MediatR;
using UserManagement.Application.Common.Interfaces.IDivision;
using UserManagement.Application.Divisions.Queries.GetUnitsByDivision;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Division.Queries
{
    public sealed class GetUnitsByDivisionQueryHandlerTests
    {
        private readonly Mock<IDivisionQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetUnitsByDivisionQueryHandler CreateSut() => new(_mockRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsUnits()
        {
            var data = new List<GetUnitsByDivisionDto> { new() { UnitId = 1 } };
            _mockRepo.Setup(r => r.GetUnitsByDivisionAsync(1, 1)).ReturnsAsync(data);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetUnitsByDivisionQuery { CompanyId = 1, DivisionId = 1 }, CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
