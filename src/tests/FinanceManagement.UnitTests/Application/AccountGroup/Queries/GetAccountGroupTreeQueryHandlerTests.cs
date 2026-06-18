using Contracts.Interfaces;
using FinanceManagement.Application.AccountGroup.Dto;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupTree;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;

namespace FinanceManagement.UnitTests.Application.AccountGroup.Queries
{
    public sealed class GetAccountGroupTreeQueryHandlerTests
    {
        private readonly Mock<IAccountGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAccountGroupTreeQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsTreeWithRootCount()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            var roots = new List<AccountGroupTreeDto>
            {
                new() { Id = 1, GroupCode = "A", Level = 1 },
                new() { Id = 2, GroupCode = "L", Level = 1 }
            };
            _mockQueryRepo.Setup(r => r.GetTreeAsync(It.IsAny<int?>())).ReturnsAsync(roots);

            var result = await CreateSut().Handle(new GetAccountGroupTreeQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_PassesTokenCompanyIdToRepository()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(7);
            _mockQueryRepo.Setup(r => r.GetTreeAsync(7)).ReturnsAsync(new List<AccountGroupTreeDto>());

            await CreateSut().Handle(new GetAccountGroupTreeQuery(), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetTreeAsync(7), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.GetTreeAsync(It.IsAny<int?>())).ReturnsAsync(new List<AccountGroupTreeDto>());

            await CreateSut().Handle(new GetAccountGroupTreeQuery(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
