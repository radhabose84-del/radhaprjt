using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.IEntity;
using UserManagement.Application.Entity.Queries.GetEntityBasedCompany;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Entity.Queries
{
    public sealed class GetEntityBasedCompanyQueryHandlerTests
    {
        private readonly Mock<IEntityQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetEntityBasedCompanyQueryHandler CreateSut() => new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsCompanyList()
        {
            var data = new List<GetEntityBasedCompanyDto> { new() { CompanyId = 1 } };
            _mockRepo.Setup(r => r.GetCompanyNames(1)).ReturnsAsync(data);
            _mockMapper.Setup(m => m.Map<List<GetEntityBasedCompanyDto>>(data)).Returns(data);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetEntityBasedCompanyQuery { EntityId = 1 }, CancellationToken.None);
            result.Should().HaveCount(1);
        }
    }
}
