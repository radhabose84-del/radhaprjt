using AutoMapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using MaintenanceManagement.Application.Common.Interfaces.IMachineGroupUser;
using MaintenanceManagement.Application.MachineGroupUser.Queries.GetMachineGroupUserAutoComplete;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MachineGroupUser.Queries.Batch2
{
    public sealed class GetMachineGroupUserAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IMachineGroupUserQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDepartmentLookup = new(MockBehavior.Loose);

        private GetMachineGroupUserAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockDepartmentLookup.Object);

        private void SetupHappyPath(List<MachineGroupUserAutoCompleteDto>? dtos = null)
        {
            _mockQueryRepo
                .Setup(r => r.GetMachineGroupUserByName(It.IsAny<string>()))
                .ReturnsAsync(new List<MachineGroupUserAutoCompleteDto>());
            _mockMapper
                .Setup(m => m.Map<List<MachineGroupUserAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(dtos ?? new List<MachineGroupUserAutoCompleteDto>());
            _mockDepartmentLookup
                .Setup(d => d.GetAllDepartmentAsync())
                .ReturnsAsync(new List<DepartmentLookupDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsMappedList()
        {
            SetupHappyPath(new List<MachineGroupUserAutoCompleteDto> { new() { DepartmentId = 1 } });

            var result = await CreateSut().Handle(
                new GetMachineGroupUserAutoCompleteQuery { SearchPattern = "abc" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_PopulatesDepartmentNameFromLookup()
        {
            _mockQueryRepo
                .Setup(r => r.GetMachineGroupUserByName(It.IsAny<string>()))
                .ReturnsAsync(new List<MachineGroupUserAutoCompleteDto>());
            _mockMapper
                .Setup(m => m.Map<List<MachineGroupUserAutoCompleteDto>>(It.IsAny<object>()))
                .Returns(new List<MachineGroupUserAutoCompleteDto>
                {
                    new() { DepartmentId = 1 }
                });
            _mockDepartmentLookup
                .Setup(d => d.GetAllDepartmentAsync())
                .ReturnsAsync(new List<DepartmentLookupDto>
                {
                    new() { DepartmentId = 1, DepartmentName = "HR" }
                });
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetMachineGroupUserAutoCompleteQuery { SearchPattern = "abc" },
                CancellationToken.None);

            result[0].DepartmentName.Should().Be("HR");
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new GetMachineGroupUserAutoCompleteQuery { SearchPattern = "abc" },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "MachineGroup User"),
                               It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NullSearch_TreatedAsEmptyString()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new GetMachineGroupUserAutoCompleteQuery { SearchPattern = null },
                CancellationToken.None);
            _mockQueryRepo.Verify(r => r.GetMachineGroupUserByName(string.Empty), Times.Once);
        }
    }
}
