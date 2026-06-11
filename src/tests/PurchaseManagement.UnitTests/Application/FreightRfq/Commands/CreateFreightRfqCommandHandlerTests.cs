using AutoMapper;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Common;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IFreightRfq;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.FreightRfq.Commands.CreateFreightRfq;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.FreightRfq.Commands
{
    public sealed class CreateFreightRfqCommandHandlerTests
    {
        private readonly Mock<IFreightRfqCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IAppDataMiscMasterLookup> _mockAppDataMisc = new(MockBehavior.Loose);

        private CreateFreightRfqCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockDocSeq.Object, _mockIp.Object, _mockOutbox.Object, _mockAppDataMisc.Object);

        private void SetupHappyPath(int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>(It.IsAny<object>()))
                .Returns(FreightRfqBuilders.ValidEntity(newId));
            _mockIp.Setup(s => s.GetUnitId()).Returns(37);
            _mockIp.Setup(s => s.GetUserName()).Returns("tester");
            _mockDocSeq.Setup(s => s.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(59);
            _mockDocSeq.Setup(s => s.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync((IReadOnlyList<string>)new List<string> { "FRFQ/2026/0001" });
            _mockAppDataMisc.Setup(s => s.GetMiscMasterByNameAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new MiscMasterLookupDto { Id = 1047 });
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newId);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccessWithNewId()
        {
            SetupHappyPath(newId: 42);

            var result = await CreateSut().Handle(FreightRfqBuilders.ValidCreateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            SetupHappyPath();

            await CreateSut().Handle(FreightRfqBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<PurchaseManagement.Domain.Entities.FreightRfq.FreightRfqHeader>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_GeneratesNumberFromDocumentSequence()
        {
            SetupHappyPath();

            await CreateSut().Handle(FreightRfqBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockDocSeq.Verify(s => s.GenerateDocumentNumber(59), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_EmailsEachTransporter()
        {
            SetupHappyPath();

            await CreateSut().Handle(FreightRfqBuilders.ValidCreateCommand(), CancellationToken.None);

            // Two transporters with email in the valid command.
            _mockOutbox.Verify(
                o => o.ScheduleAsync(It.IsAny<Contracts.Events.Notifications.NotificationCreatedEvent>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();

            await CreateSut().Handle(FreightRfqBuilders.ValidCreateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
