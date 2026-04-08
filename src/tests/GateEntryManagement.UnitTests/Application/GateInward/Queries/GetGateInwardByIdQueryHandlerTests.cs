using AutoMapper;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Application.GateInward.Dto;
using GateEntryManagement.Application.GateInward.Queries.GetGateInwardById;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.GateInward.Queries
{
    public sealed class GetGateInwardByIdQueryHandlerTests
    {
        private readonly Mock<IGateInwardQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetGateInwardByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsDto()
        {
            var dto = new GateInwardHdrDto
            {
                Id = 1,
                GateEntryNo = "GE001",
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
                new GetGateInwardByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.GateEntryNo.Should().Be("GE001");
        }

        [Fact]
        public async Task Handle_ReturnsNull_WhenNotFound()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((GateInwardHdrDto?)null);

            var result = await CreateSut().Handle(
                new GetGateInwardByIdQuery { Id = 999 },
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent_WhenFound()
        {
            var dto = new GateInwardHdrDto { Id = 1, GateEntryNo = "GE001" };
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetGateInwardByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetById" &&
                        e.ActionCode == "GetGateInwardByIdQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DoesNotPublishAuditEvent_WhenNotFound()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((GateInwardHdrDto?)null);

            await CreateSut().Handle(
                new GetGateInwardByIdQuery { Id = 999 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
