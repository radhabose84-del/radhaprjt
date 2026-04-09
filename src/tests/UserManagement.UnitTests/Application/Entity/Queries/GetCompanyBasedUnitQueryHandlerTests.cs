using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IEntity;
using UserManagement.Application.Entity.Queries.GetCompanyBasedUnit;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Entity.Queries
{
    public sealed class GetCompanyBasedUnitQueryHandlerTests
    {
        private readonly Mock<IEntityQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCompanyBasedUnitQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsUnitList()
        {
            var data = new List<GetCompanyBasedUnitDto> { new() { UnitId = 1 } };
            _mockRepo.Setup(r => r.GetCompanyBasedUnits(It.IsAny<List<int>>())).ReturnsAsync(data);
            _mockMapper.Setup(m => m.Map<List<GetCompanyBasedUnitDto>>(data)).Returns(data);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetCompanyBasedUnitQuery { CompanyIds = new List<int> { 1 } }, CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
