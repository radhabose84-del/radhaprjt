using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Dto;
using InventoryManagement.Application.PriceGroupMaster.Queries.GetPriceGroupMasterById;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.PriceGroupMaster.Queries
{
    public sealed class GetPriceGroupMasterByIdQueryHandlerTests
    {
        private readonly Mock<IPriceGroupMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPriceGroupMasterByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var dto = new PriceGroupMasterDto { Id = 10, PriceGroupCode = "PG10" };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(new GetPriceGroupMasterByIdQuery { Id = 10 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(10);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((PriceGroupMasterDto?)null);

            var result = await CreateSut().Handle(new GetPriceGroupMasterByIdQuery { Id = 999 }, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var dto = new PriceGroupMasterDto { Id = 5 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(dto);

            await CreateSut().Handle(new GetPriceGroupMasterByIdQuery { Id = 5 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "PRICEGROUP_GETBYID"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_DoesNotPublishAuditEvent()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((PriceGroupMasterDto?)null);

            await CreateSut().Handle(new GetPriceGroupMasterByIdQuery { Id = 99 }, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
