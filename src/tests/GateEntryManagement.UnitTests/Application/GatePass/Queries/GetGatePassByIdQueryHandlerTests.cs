using AutoMapper;
using GateEntryManagement.Application.Common.Interfaces.IGatePass;
using GateEntryManagement.Application.GatePass.Dto;
using GateEntryManagement.Application.GatePass.Queries.GetGatePassById;
using GateEntryManagement.Domain.Events;
using MediatR;

namespace GateEntryManagement.UnitTests.Application.GatePass.Queries
{
    public sealed class GetGatePassByIdQueryHandlerTests
    {
        private readonly Mock<IGatePassQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetGatePassByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsDto()
        {
            var dto = new GatePassHdrDto
            {
                Id = 1,
                GatePassNo = "GP001",
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
                new GetGatePassByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result.GatePassNo.Should().Be("GP001");
        }

        [Fact]
        public async Task Handle_ReturnsNull_WhenNotFound()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((GatePassHdrDto?)null);

            var result = await CreateSut().Handle(
                new GetGatePassByIdQuery { Id = 999 },
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent_WhenFound()
        {
            var dto = new GatePassHdrDto { Id = 1, GatePassNo = "GP001" };
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(dto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetGatePassByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetById" &&
                        e.ActionCode == "GetGatePassByIdQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DoesNotPublishAuditEvent_WhenNotFound()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((GatePassHdrDto?)null);

            await CreateSut().Handle(
                new GetGatePassByIdQuery { Id = 999 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
