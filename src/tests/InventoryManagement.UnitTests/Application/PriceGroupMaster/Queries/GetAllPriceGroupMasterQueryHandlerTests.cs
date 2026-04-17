using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Dto;
using InventoryManagement.Application.PriceGroupMaster.Queries.GetAllPriceGroupMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.PriceGroupMaster.Queries
{
    public sealed class GetAllPriceGroupMasterQueryHandlerTests
    {
        private readonly Mock<IPriceGroupMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllPriceGroupMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsPaginatedResponse()
        {
            var data = new List<PriceGroupMasterDto> { new() { Id = 1, PriceGroupCode = "PG1" } };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((data, 1));

            var result = await CreateSut().Handle(
                new GetAllPriceGroupMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
            result.PageNumber.Should().Be(1);
            result.PageSize.Should().Be(10);
        }

        [Fact]
        public async Task Handle_PassesSearchTerm_ToRepository()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(2, 5, "abc"))
                .ReturnsAsync((new List<PriceGroupMasterDto>(), 0));

            await CreateSut().Handle(
                new GetAllPriceGroupMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "abc" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(2, 5, "abc"), Times.Once);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .ReturnsAsync((new List<PriceGroupMasterDto>(), 0));

            await CreateSut().Handle(new GetAllPriceGroupMasterQuery(), CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "PRICEGROUP_GETALL"),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
