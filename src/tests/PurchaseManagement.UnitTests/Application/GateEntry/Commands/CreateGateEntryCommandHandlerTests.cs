using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGateEntry;
using PurchaseManagement.Application.GRN.GateEntry.Commands.CreateGateEntry;
using PurchaseManagement.Domain.Entities.GRN.GateEntry;
using PurchaseManagement.Domain.Events;

namespace PurchaseManagement.UnitTests.Application.GateEntry.Commands
{
    public sealed class CreateGateEntryCommandHandlerTests
    {
        private readonly Mock<IGateEntryCommandRepository> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IGateEntryQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private CreateGateEntryCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = new CreateGateEntryCommand
            {
                GateEntryDetails = new CreateGateEntryDto
                {
                    PartyId = 1,
                    UnitId = 1,
                    VehicleNumber = "KA01AB1234",
                    ReceivingTypeId = 1
                }
            };

            var header = new GateEntryHeader
            {
                GateEntryNo = "GE001",
                GateEntryDate = DateTimeOffset.Now,
                PartyId = 1,
                UnitId = 1
            };

            _mockMapper
                .Setup(m => m.Map<GateEntryHeader>(command.GateEntryDetails))
                .Returns(header);

            _mockCommandRepo
                .Setup(r => r.GenerateNextCodeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("GE002");

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<GateEntryHeader>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = new CreateGateEntryCommand
            {
                GateEntryDetails = new CreateGateEntryDto
                {
                    PartyId = 1,
                    UnitId = 1,
                    VehicleNumber = "KA01AB1234",
                    ReceivingTypeId = 1
                }
            };

            var header = new GateEntryHeader
            {
                GateEntryNo = "GE001",
                GateEntryDate = DateTimeOffset.Now,
                PartyId = 1,
                UnitId = 1
            };

            _mockMapper
                .Setup(m => m.Map<GateEntryHeader>(command.GateEntryDetails))
                .Returns(header);

            _mockCommandRepo
                .Setup(r => r.GenerateNextCodeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("GE002");

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<GateEntryHeader>()))
                .ReturnsAsync(1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<GateEntryHeader>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = new CreateGateEntryCommand
            {
                GateEntryDetails = new CreateGateEntryDto
                {
                    PartyId = 1,
                    UnitId = 1,
                    VehicleNumber = "KA01AB1234",
                    ReceivingTypeId = 1
                }
            };

            var header = new GateEntryHeader
            {
                GateEntryNo = "GE001",
                GateEntryDate = DateTimeOffset.Now,
                PartyId = 1,
                UnitId = 1
            };

            _mockMapper
                .Setup(m => m.Map<GateEntryHeader>(command.GateEntryDetails))
                .Returns(header);

            _mockCommandRepo
                .Setup(r => r.GenerateNextCodeAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync("GE002");

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<GateEntryHeader>()))
                .ReturnsAsync(1);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Create"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
