using Contracts.Interfaces;
using FinanceManagement.Application.AccountAuditTrail.Dto;
using FinanceManagement.Application.AccountAuditTrail.Queries.GetAccountAuditHistory;
using FinanceManagement.Application.Common.Interfaces.IAccountAuditTrail;

namespace FinanceManagement.UnitTests.Application.AccountAuditTrail
{
    public sealed class GetAccountAuditHistoryQueryHandlerTests
    {
        private readonly Mock<IAccountAuditTrailQueryRepository> _mockRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAccountAuditHistoryQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockIp.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsRepositoryRows_UsingCompanyFromToken()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(7);
            var rows = new List<AccountAuditTrailDto>
            {
                new() { Id = 1, EntityName = "GlAccountMaster", EntityId = 42, Action = "Update",
                        PropertyName = "Description", OldValue = "1", NewValue = "-1" }
            };
            _mockRepo.Setup(r => r.GetHistoryAsync(7, "GlAccountMaster", 42, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rows);

            var result = await CreateSut().Handle(
                new GetAccountAuditHistoryQuery("GlAccountMaster", 42), CancellationToken.None);

            result.Should().BeSameAs(rows);
            _mockRepo.Verify(r => r.GetHistoryAsync(7, "GlAccountMaster", 42, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NoHistory_ReturnsEmpty()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockRepo.Setup(r => r.GetHistoryAsync(1, "AccountGroup", 9, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AccountAuditTrailDto>());

            var result = await CreateSut().Handle(
                new GetAccountAuditHistoryQuery("AccountGroup", 9), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockRepo.Setup(r => r.GetHistoryAsync(1, "GlAccountMaster", 42, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<AccountAuditTrailDto>());

            await CreateSut().Handle(new GetAccountAuditHistoryQuery("GlAccountMaster", 42), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
