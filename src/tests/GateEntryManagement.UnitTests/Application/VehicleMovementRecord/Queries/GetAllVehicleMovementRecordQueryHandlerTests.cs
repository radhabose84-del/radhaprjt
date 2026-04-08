using AutoMapper;
using Contracts.Common;
using GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord;
using GateEntryManagement.Application.VehicleMovementRecord.Dto;
using GateEntryManagement.Application.VehicleMovementRecord.Queries.GetAllVehicleMovementRecord;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.VehicleMovementRecord.Queries
{
    public sealed class GetAllVehicleMovementRecordQueryHandlerTests
    {
        private readonly Mock<IVehicleMovementRecordQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllVehicleMovementRecordQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<VehicleMovementRecordDto>
            {
                new VehicleMovementRecordDto { Id = 1, VehicleMovementId = "VMR001", VehicleNumber = "KA01AB1234" }
            };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((dtoList, 1));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAllVehicleMovementRecordQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<VehicleMovementRecordDto>
            {
                new VehicleMovementRecordDto { Id = 1, VehicleMovementId = "VMR001" }
            };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(2, 5, "search"))
                .ReturnsAsync((dtoList, 11));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAllVehicleMovementRecordQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<VehicleMovementRecordDto>(), 0));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAllVehicleMovementRecordQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<VehicleMovementRecordDto>(), 0));

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetAllVehicleMovementRecordQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetAllVehicleMovementRecordQuery" &&
                        e.ActionCode == "Get"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
