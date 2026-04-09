using AutoMapper;
using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Dto;
using GateEntryManagement.Application.VehicleMovementRecord.Queries.GetPendingVehicle;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.VehicleMovementRecord.Queries
{
    public sealed class GetPendingVehicleQueryHandlerTests
    {
        private readonly Mock<IVehicleMovementRecordQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPendingVehicleQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<PendingVehicleDto>
            {
                new PendingVehicleDto
                {
                    Id = 1,
                    VehicleMovementId = "VMR001",
                    VehicleNumber = "KA01AB1234",
                    DriverName = "Test Driver",
                    GateInTime = DateTimeOffset.UtcNow
                }
            };
            _mockQueryRepo
                .Setup(r => r.GetPendingVehiclesAsync(null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetPendingVehicleQuery(),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetPendingVehiclesAsync(null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PendingVehicleDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetPendingVehicleQuery(),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithFilters_PassesParametersToRepo()
        {
            _mockQueryRepo
                .Setup(r => r.GetPendingVehiclesAsync("VMR001", "KA01AB1234", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PendingVehicleDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetPendingVehicleQuery { VehicleMovementId = "VMR001", VehicleNumber = "KA01AB1234" },
                CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetPendingVehiclesAsync("VMR001", "KA01AB1234", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetPendingVehiclesAsync(null, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<PendingVehicleDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetPendingVehicleQuery(),
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetPendingVehicleQuery" &&
                        e.ActionCode == "Get"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
