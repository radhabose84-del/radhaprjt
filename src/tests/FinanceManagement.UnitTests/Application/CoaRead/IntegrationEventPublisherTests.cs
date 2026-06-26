using Contracts.Events.Coa;
using FinanceManagement.Application.Common.Interfaces.IOutbox;
using FinanceManagement.Infrastructure.Services;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FinanceManagement.UnitTests.Application.CoaRead
{
    // US-GL02-16 (AC3) — direct bus publish on the happy path; transactional-outbox fallback on failure.
    public sealed class IntegrationEventPublisherTests
    {
        private readonly Mock<IPublishEndpoint> _bus = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _outbox = new(MockBehavior.Loose);
        private readonly Mock<ILogger<IntegrationEventPublisher>> _log = new(MockBehavior.Loose);

        private IntegrationEventPublisher Sut() => new(_bus.Object, _outbox.Object, _log.Object);
        private static GlAccountDeactivatedEvent Evt() => new() { AccountId = 5, AccountCode = "1001" };

        [Fact]
        public async Task HappyPath_PublishesToBus_NoOutbox()
        {
            await Sut().PublishWithinSlaAsync(Evt(), Guid.NewGuid(), CancellationToken.None);

            _bus.Verify(b => b.Publish(It.IsAny<GlAccountDeactivatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
            _outbox.Verify(o => o.ScheduleAsync(It.IsAny<GlAccountDeactivatedEvent>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task BusDown_FallsBackToOutbox()
        {
            _bus.Setup(b => b.Publish(It.IsAny<GlAccountDeactivatedEvent>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("broker down"));

            await Sut().PublishWithinSlaAsync(Evt(), Guid.NewGuid(), CancellationToken.None);

            _outbox.Verify(o => o.ScheduleAsync(It.IsAny<GlAccountDeactivatedEvent>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
