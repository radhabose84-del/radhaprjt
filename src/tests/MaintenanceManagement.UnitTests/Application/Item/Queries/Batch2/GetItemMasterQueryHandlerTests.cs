using AutoMapper;
using MaintenanceManagement.Application.Common.Interfaces.IItem;
using MaintenanceManagement.Application.Item.ItemMaster.Queries;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.Item.Queries.Batch2
{
    public sealed class GetItemMasterQueryHandlerTests
    {
        private readonly Mock<IItemQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetItemMasterQueryHandler CreateSut() =>
            new(_mockMapper.Object, _mockMediator.Object, _mockQueryRepo.Object);

        private void SetupHappyPath(List<GetItemMasterDto>? dtos = null)
        {
            _mockQueryRepo
                .Setup(r => r.GetItemMasters(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(new List<MaintenanceManagement.Domain.Entities.ItemMaster>());
            _mockMapper
                .Setup(m => m.Map<List<GetItemMasterDto>>(It.IsAny<object>()))
                .Returns(dtos ?? new List<GetItemMasterDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsMappedList()
        {
            SetupHappyPath(new List<GetItemMasterDto> { new() { ItemCode = "I01", ItemName = "Item1" } });

            var result = await CreateSut().Handle(
                new GetItemMasterQuery { OldUnitId = "U01", Grpcode = "G01", ItemCode = "I01", ItemName = "Item1" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new GetItemMasterQuery { OldUnitId = "U01", Grpcode = "G01" },
                CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetItemMasters("U01", "G01", null, null),
                Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new GetItemMasterQuery { OldUnitId = "U01", Grpcode = "G01" }, CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.Module == "ItemMaster"),
                               It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new GetItemMasterQuery { OldUnitId = "U01", Grpcode = "G01" }, CancellationToken.None);
            result.Should().BeEmpty();
        }
    }
}
