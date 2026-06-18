using Contracts.Interfaces;
using FinanceManagement.Application.AccountGroup.Dto;
using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupLeaves;
using FinanceManagement.Application.Common.Interfaces.IAccountGroup;

namespace FinanceManagement.UnitTests.Application.AccountGroup.Queries
{
    public sealed class GetAccountGroupLeavesQueryHandlerTests
    {
        private readonly Mock<IAccountGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAccountGroupLeavesQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockIp.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLeafGroups()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            IReadOnlyList<AccountGroupLookupDto> leaves = new List<AccountGroupLookupDto>
            {
                new() { Id = 9, GroupCode = "A-CA-OCA-PRE", GroupName = "Prepaid Expenses", Level = 4 }
            };
            _mockQueryRepo
                .Setup(r => r.GetLeafGroupsAsync(It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(leaves);

            var result = await CreateSut().Handle(new GetAccountGroupLeavesQuery(null), CancellationToken.None);

            result.Should().ContainSingle();
        }

        [Fact]
        public async Task Handle_PassesTokenCompanyAndAccountTypeToRepository()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(7);
            _mockQueryRepo
                .Setup(r => r.GetLeafGroupsAsync(7, 3))
                .ReturnsAsync(new List<AccountGroupLookupDto>());

            await CreateSut().Handle(new GetAccountGroupLeavesQuery(3), CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetLeafGroupsAsync(7, 3), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo
                .Setup(r => r.GetLeafGroupsAsync(It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<AccountGroupLookupDto>());

            await CreateSut().Handle(new GetAccountGroupLeavesQuery(null), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
