using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroup;
using MaintenanceManagement.Application.MachineGroup.Queries.GetMachineGroupAutoComplete;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineGroup.Queries.Batch2
{
    public sealed class GetMachineGroupAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMachineGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetMachineGroupAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(List<GetMachineGroupAutoCompleteDto>? dtos = null)
        {
            _mockQueryRepo
                .Setup(r => r.GetMachineGroupAutoComplete(It.IsAny<string>()))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.MachineGroup>());
            _mockMapper
                .Setup(m => m.Map<List<GetMachineGroupAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos ?? new List<GetMachineGroupAutoCompleteDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsMappedList()
        {
            SetupHappyPath(new List<GetMachineGroupAutoCompleteDto> { new() });

            var result = await CreateSut().Handle(
                new GetMachineGroupAutoCompleteQuery { SearchPattern = "abc" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new GetMachineGroupAutoCompleteQuery { SearchPattern = "abc" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetMachineGroupAutoComplete("abc"), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new GetMachineGroupAutoCompleteQuery { SearchPattern = "abc" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "MachineGroup"),
                               It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(
                new GetMachineGroupAutoCompleteQuery { SearchPattern = "x" },
                CancellationToken.None);
            result.Should().BeEmpty();
        }
    }
}
