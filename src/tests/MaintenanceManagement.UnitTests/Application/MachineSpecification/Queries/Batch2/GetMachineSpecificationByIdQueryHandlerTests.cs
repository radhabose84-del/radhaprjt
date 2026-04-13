using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMachineSpecification;
using MaintenanceManagement.Application.MachineSpecification.Command;
using MaintenanceManagement.Application.MachineSpecification.Queries.GetMachineSpecificationById;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineSpecification.Queries.Batch2
{
    public sealed class GetMachineSpecificationByIdQueryHandlerTests
    {
        private readonly Mock<IMachineSpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMachineSpecificationByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new List<MachineSpecificationDto> { new() { Id = 1 } });
            _mockMapper
                .Setup(m => m.Map<List<MachineSpecificationDto>>(It.IsAny<object>()))
                .Returns(new List<MachineSpecificationDto> { new() { Id = 1 } });
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMachineSpecificationByIdQuery { Id = 1 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync(new List<MachineSpecificationDto>());

            var result = await CreateSut().Handle(
                new GetMachineSpecificationByIdQuery { Id = 99 },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_NullResult_ReturnsFailure()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((List<MachineSpecificationDto>)null!);

            var result = await CreateSut().Handle(
                new GetMachineSpecificationByIdQuery { Id = 42 },
                CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_Success_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new List<MachineSpecificationDto> { new() { Id = 1 } });
            _mockMapper
                .Setup(m => m.Map<List<MachineSpecificationDto>>(It.IsAny<object>()))
                .Returns(new List<MachineSpecificationDto> { new() { Id = 1 } });
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetMachineSpecificationByIdQuery { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "MachineSpecification"),
                               It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
