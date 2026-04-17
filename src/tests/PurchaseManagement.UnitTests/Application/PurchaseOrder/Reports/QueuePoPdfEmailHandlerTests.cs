using Contracts.Dtos.Common;
using Contracts.Events.Notifications;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder;
using PurchaseManagement.Application.PurchaseOrder.Reports;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.Reports
{
    public sealed class QueuePoPdfEmailHandlerTests
    {
        private readonly Mock<ISsrsClient> _mockSsrs = new(MockBehavior.Loose);
        private readonly Mock<IFileStorage> _mockStorage = new(MockBehavior.Loose);
        private readonly Mock<IPublishEndpoint> _mockBus = new(MockBehavior.Loose);
        private readonly Mock<IConfiguration> _mockCfg = new(MockBehavior.Loose);
        private readonly Mock<ILogger<QueuePoPdfEmailHandler>> _mockLogger = new(MockBehavior.Loose);

        private QueuePoPdfEmailHandler CreateSut() =>
            new(_mockSsrs.Object, _mockStorage.Object, _mockBus.Object, _mockCfg.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_NoRecipients_ReturnsCorrelationId()
        {
            var command = new QueuePoPdfEmailCommand(1, 100, new List<PartyRefDto>(), "");
            _mockSsrs.Setup(s => s.RenderPdfAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, string?>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new byte[] { 1, 2, 3 });
            _mockStorage.Setup(s => s.PutAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://blob/test.pdf");
            _mockCfg.Setup(c => c["Ssrs:PoReportPath"]).Returns("/bsoft/PO");

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBe(Guid.Empty);
            _mockBus.Verify(b => b.Publish(It.IsAny<NotificationCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithRecipients_PublishesPerRecipient()
        {
            var contacts = new List<PartyRefDto>
            {
                new PartyRefDto { Email = "a@test.com", Name = "A" },
                new PartyRefDto { Email = "b@test.com", Name = "B" }
            };
            var command = new QueuePoPdfEmailCommand(1, 100, contacts, "");
            _mockSsrs.Setup(s => s.RenderPdfAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, string?>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new byte[] { 1, 2, 3 });
            _mockStorage.Setup(s => s.PutAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://blob/test.pdf");
            _mockCfg.Setup(c => c["Ssrs:PoReportPath"]).Returns("/bsoft/PO");

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBe(Guid.Empty);
            _mockBus.Verify(b => b.Publish(It.IsAny<NotificationCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_DuplicateEmails_DeduplicatesRecipients()
        {
            var contacts = new List<PartyRefDto>
            {
                new PartyRefDto { Email = "same@test.com", Name = "A" },
                new PartyRefDto { Email = "same@test.com", Name = "B" }
            };
            var command = new QueuePoPdfEmailCommand(1, 100, contacts, "");
            _mockSsrs.Setup(s => s.RenderPdfAsync(It.IsAny<string>(), It.IsAny<IDictionary<string, string?>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new byte[] { 1, 2, 3 });
            _mockStorage.Setup(s => s.PutAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://blob/test.pdf");
            _mockCfg.Setup(c => c["Ssrs:PoReportPath"]).Returns("/bsoft/PO");

            await CreateSut().Handle(command, CancellationToken.None);

            _mockBus.Verify(b => b.Publish(It.IsAny<NotificationCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void CanInstantiate()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }
    }
}
