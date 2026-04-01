using ProductionManagement.Application.Common.Interfaces.IRepackingMaster;
using ProductionManagement.Application.RepackingMaster.Dto;
using ProductionManagement.Application.RepackingMaster.Queries.GetRepackingMasterById;

namespace ProductionManagement.UnitTests.Application.Repacking.Queries
{
    public sealed class GetRepackingMasterByIdQueryHandlerTests
    {
        private readonly Mock<IRepackingMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetRepackingMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static RepackingMasterDto BuildDto(int id = 1) => new()
        {
            Id = id,
            RepackDocNo = "REPACK-001",
            RepackDate = DateOnly.FromDateTime(DateTime.Today),
            ItemId = 1,
            ItemName = "Test Item",
            IsActive = true,
            IsDeleted = false
        };

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = BuildDto(5);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetRepackingMasterByIdQuery { Id = 5 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(5);
            result.RepackDocNo.Should().Be("REPACK-001");
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((RepackingMasterDto?)null);

            var result = await CreateSut().Handle(
                new GetRepackingMasterByIdQuery { Id = 999 },
                CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var dto = BuildDto(1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetRepackingMasterByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "GetRepackingMasterByIdQuery"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NonExistentId_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((RepackingMasterDto?)null);

            await CreateSut().Handle(
                new GetRepackingMasterByIdQuery { Id = 999 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
