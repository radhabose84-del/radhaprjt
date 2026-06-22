using FinanceManagement.Application.AccountGroup.Queries.GetAccountGroupApprovalChain;
using Microsoft.Extensions.Configuration;

namespace FinanceManagement.UnitTests.Application.AccountGroup.Queries
{
    public sealed class GetAccountGroupApprovalChainQueryHandlerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private static IConfigurationSection Child(string level, string label)
        {
            var m = new Mock<IConfigurationSection>(MockBehavior.Loose);
            m.Setup(c => c["level"]).Returns(level);
            m.Setup(c => c["label"]).Returns(label);
            return m.Object;
        }

        // Mocks IConfiguration.GetSection("Finance:MoveApprovalChain").GetChildren() — interface methods, no package.
        private static IConfiguration Config(params IConfigurationSection[] children)
        {
            var section = new Mock<IConfigurationSection>(MockBehavior.Loose);
            section.Setup(s => s.GetChildren()).Returns(children);
            var config = new Mock<IConfiguration>(MockBehavior.Loose);
            config.Setup(c => c.GetSection("Finance:MoveApprovalChain")).Returns(section.Object);
            return config.Object;
        }

        private GetAccountGroupApprovalChainQueryHandler CreateSut(IConfiguration config) =>
            new(config, _mockMediator.Object);

        [Fact]
        public async Task Handle_NoConfig_ReturnsDefaultChain_FcThenCfo()
        {
            var result = await CreateSut(Config()).Handle(new GetAccountGroupApprovalChainQuery(), CancellationToken.None);

            result.Should().HaveCount(2);
            result[0].Level.Should().Be(1);
            result[0].Label.Should().Be("Finance Controller");
            result[1].Level.Should().Be(2);
            result[1].Label.Should().Be("CFO");
        }

        [Fact]
        public async Task Handle_ConfiguredChain_ReturnsOrderedByLevel()
        {
            // Supplied out of order — handler must sort by level.
            var config = Config(Child("2", "CFO"), Child("1", "Finance Controller"));

            var result = await CreateSut(config).Handle(new GetAccountGroupApprovalChainQuery(), CancellationToken.None);

            result.Select(c => c.Label).Should().ContainInOrder("Finance Controller", "CFO");
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            await CreateSut(Config()).Handle(new GetAccountGroupApprovalChainQuery(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
