using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using GateEntryManagement.Application.Common.Interfaces.IMiscMaster;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Commands.CreateVehicleMovementRecord;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.VehicleMovementRecord.Commands
{
    public sealed class CreateVehicleMovementRecordCommandHandlerTests
    {
        private readonly Mock<IVehicleMovementRecordCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IVehicleMovementRecordQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private CreateVehicleMovementRecordCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMiscRepo.Object, _mockDocSeq.Object, _mockMediator.Object, _mockMapper.Object, _mockIpService.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            _mockIpService.Setup(i => i.GetUnitId()).Returns(1);
            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(1);
            _mockDocSeq.Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { "VMR00001" });
            _mockMiscRepo.Setup(m => m.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new GateEntryManagement.Domain.Entities.MiscMaster { Id = 1, Code = "INSIDE", Description = "Inside Premises" });
            _mockMapper.Setup(m => m.Map<GateEntryManagement.Domain.Entities.VehicleMovementRecord>(It.IsAny<CreateVehicleMovementRecordCommand>()))
                .Returns(new GateEntryManagement.Domain.Entities.VehicleMovementRecord());
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<GateEntryManagement.Domain.Entities.VehicleMovementRecord>(), It.IsAny<int>())).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new CreateVehicleMovementRecordCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }
    }
}
