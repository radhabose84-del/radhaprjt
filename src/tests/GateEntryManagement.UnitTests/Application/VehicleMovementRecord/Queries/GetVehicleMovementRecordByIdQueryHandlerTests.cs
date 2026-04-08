using AutoMapper;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Dto;
using GateEntryManagement.Application.VehicleMovementRecord.Queries.GetVehicleMovementRecordById;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.VehicleMovementRecord.Queries
{
    public sealed class GetVehicleMovementRecordByIdQueryHandlerTests
    {
        private readonly Mock<IVehicleMovementRecordQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetVehicleMovementRecordByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsDto()
        {
            var dto = new VehicleMovementRecordDto
            {
                Id = 1,
                VehicleMovementId = "VMR001",
                VehicleNumber = "KA01AB1234",
                DriverName = "Test Driver"
            };
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetVehicleMovementRecordByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.VehicleMovementId.Should().Be("VMR001");
        }

        [Fact]
        public async Task Handle_ReturnsNull_WhenNotFound()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((VehicleMovementRecordDto?)null);

            var result = await CreateSut().Handle(
                new GetVehicleMovementRecordByIdQuery { Id = 999 },
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent_WhenFound()
        {
            var dto = new VehicleMovementRecordDto { Id = 1, VehicleMovementId = "VMR001" };
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetVehicleMovementRecordByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetById" &&
                        e.ActionCode == "GetVehicleMovementRecordByIdQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DoesNotPublishAuditEvent_WhenNotFound()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((VehicleMovementRecordDto?)null);

            await CreateSut().Handle(
                new GetVehicleMovementRecordByIdQuery { Id = 999 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
