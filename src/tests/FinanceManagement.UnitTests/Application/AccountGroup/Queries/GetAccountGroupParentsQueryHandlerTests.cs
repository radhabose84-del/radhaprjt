using Contracts.Interfaces;
using FinanceManagement.Application.AccountGroup.Dto;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupParents;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;

namespace FinanceManagement.UnitTests.Application.AccountGroup.Queries
{
    public sealed class GetAccountGroupParentsQueryHandlerTests
    {
        private readonly Mock<IAccountGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAccountGroupParentsQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsParentsForLevel()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            IReadOnlyList<AccountGroupLookupDto> parents = new List<AccountGroupLookupDto>
            {
                new() { Id = 2, GroupCode = "A-CA", GroupName = "Current Assets", Level = 2 }
            };
            _mockQueryRepo
                .Setup(r => r.GetParentsByLevelAsync(It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(parents);

            var result = await CreateSut().Handle(new GetAccountGroupParentsQuery(2), CancellationToken.None);

            result.Should().ContainSingle();
        }

        [Fact]
        public async Task Handle_PassesLevelAndTokenCompanyToRepository()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(7);
            _mockQueryRepo
                .Setup(r => r.GetParentsByLevelAsync(2, 7))
                .ReturnsAsync(new List<AccountGroupLookupDto>());

            await CreateSut().Handle(new GetAccountGroupParentsQuery(2), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetParentsByLevelAsync(2, 7), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo
                .Setup(r => r.GetParentsByLevelAsync(It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<AccountGroupLookupDto>());

            await CreateSut().Handle(new GetAccountGroupParentsQuery(2), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
