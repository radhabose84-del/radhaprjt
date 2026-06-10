using AutoMapper;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Dto;
using GateEntryManagement.Application.VehicleMovementRecord.Queries.GetVehicleMovementRecordAutoComplete;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.VehicleMovementRecord.Queries
{
    public sealed class GetVehicleMovementRecordAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IVehicleMovementRecordQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetVehicleMovementRecordAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsResults()
        {
            var lookupList = new List<VehicleMovementRecordAutoCompleteDto>
            {
                new VehicleMovementRecordAutoCompleteDto { Id = 1, VehicleMovementId = "VMR001", VehicleNumber = "KA01AB1234" }
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("KA01", It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetVehicleMovementRecordAutoCompleteQuery("KA01"),
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].VehicleNumber.Should().Be("KA01AB1234");
        }

        [Fact]
        public async Task Handle_EmptyTerm_ReturnsResults()
        {
            var lookupList = new List<VehicleMovementRecordAutoCompleteDto>
            {
                new VehicleMovementRecordAutoCompleteDto { Id = 1, VehicleMovementId = "VMR001" },
                new VehicleMovementRecordAutoCompleteDto { Id = 2, VehicleMovementId = "VMR002" }
            };
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("", It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(lookupList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetVehicleMovementRecordAutoCompleteQuery(null!),
                CancellationToken.None);

            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.AutocompleteAsync("Test", It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<VehicleMovementRecordAutoCompleteDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetVehicleMovementRecordAutoCompleteQuery("Test"),
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetAll" &&
                        e.ActionCode == "GetVehicleMovementRecordAutoCompleteQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
