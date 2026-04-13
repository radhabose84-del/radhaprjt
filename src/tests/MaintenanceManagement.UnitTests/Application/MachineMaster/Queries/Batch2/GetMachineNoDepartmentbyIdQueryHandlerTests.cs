using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMachineMaster;
using MaintenanceManagement.Application.MachineMaster.Queries.GetMachineNoDepartmentbyId;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineMaster.Queries.Batch2
{
    public sealed class GetMachineNoDepartmentbyIdQueryHandlerTests
    {
        private readonly Mock<IMachineMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMachineNoDepartmentbyIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMappedList()
        {
            _mockQueryRepo
                .Setup(r => r.GetMachineNoDepartmentAsync(1))
                .ReturnsAsync(new List<GetMachineNoDepartmentbyIdDto> { new() { Id = 1, MachineCode = "M01" } });
            _mockMapper
                .Setup(m => m.Map<List<GetMachineNoDepartmentbyIdDto>>(It.IsAny<object>()))
                .Returns(new List<GetMachineNoDepartmentbyIdDto> { new() { Id = 1, MachineCode = "M01" } });

            var result = await CreateSut().Handle(
                new GetMachineNoDepartmentbyIdQuery { DepartmentId = 1 },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetMachineNoDepartmentAsync(999))
                .ReturnsAsync(new List<GetMachineNoDepartmentbyIdDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetMachineNoDepartmentbyIdDto>>(It.IsAny<object>()))
                .Returns(new List<GetMachineNoDepartmentbyIdDto>());

            var result = await CreateSut().Handle(
                new GetMachineNoDepartmentbyIdQuery { DepartmentId = 999 },
                CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetMachineNoDepartmentAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<GetMachineNoDepartmentbyIdDto>());
            _mockMapper
                .Setup(m => m.Map<List<GetMachineNoDepartmentbyIdDto>>(It.IsAny<object>()))
                .Returns(new List<GetMachineNoDepartmentbyIdDto>());

            await CreateSut().Handle(
                new GetMachineNoDepartmentbyIdQuery { DepartmentId = 1 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "MachineMaster"),
                               It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
