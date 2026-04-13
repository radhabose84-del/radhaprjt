using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IItem;
using MaintenanceManagement.Application.Item.ItemGroup.Queries;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.Item.Queries.Batch2
{
    public sealed class GetItemGroupQueryHandlerTests
    {
        private readonly Mock<IItemQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetItemGroupQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(List<MaintenanceManagement.Domain.Entities.ItemGroupCode>? entities = null,
                                    List<GetItemGroupDto>? dtos = null)
        {
            entities ??= new List<MaintenanceManagement.Domain.Entities.ItemGroupCode>();
            dtos ??= new List<GetItemGroupDto>();

            _mockQueryRepo.Setup(r => r.GetGroupCodes(It.IsAny<string>())).ReturnsAsync(entities);
            _mockMapper.Setup(m => m.Map<List<GetItemGroupDto>>(It.IsAny<object>())).Returns(dtos);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                         .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsMappedList()
        {
            var dtos = new List<GetItemGroupDto> { new() { GroupCode = "G1", GroupName = "Group" } };
            SetupHappyPath(dtos: dtos);

            var result = await CreateSut().Handle(new GetItemGroupQuery { OldUnitId = "U01" }, CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].GroupCode.Should().Be("G1");
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(new GetItemGroupQuery { OldUnitId = "U01" }, CancellationToken.None);
            _mockQueryRepo.Verify(r => r.GetGroupCodes("U01"), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new GetItemGroupQuery { OldUnitId = "U01" }, CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "ItemGroupCode"),
                               It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new GetItemGroupQuery { OldUnitId = "U01" }, CancellationToken.None);
            result.Should().BeEmpty();
        }
    }
}
