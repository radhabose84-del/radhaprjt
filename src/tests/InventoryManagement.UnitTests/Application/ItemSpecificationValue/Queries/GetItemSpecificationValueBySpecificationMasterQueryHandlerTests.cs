using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using InventoryManagement.Application.ItemSpecificationValue.Queries.GetItemSpecificationValueBySpecificationMaster;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.ItemSpecificationValue.Queries
{
    public sealed class GetItemSpecificationValueBySpecificationMasterQueryHandlerTests
    {
        private readonly Mock<IItemSpecificationValueQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetItemSpecificationValueBySpecificationMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsMappedDtos()
        {
            IReadOnlyList<ItemSpecificationValueDto> rows = new List<ItemSpecificationValueDto>
            {
                new() { Id = 1, SpecificationMasterId = 10, SpecificationValue = "Red" }
            };
            _mockQueryRepo
                .Setup(r => r.GetBySpecificationMasterIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rows);
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationValueDto>>(rows))
                .Returns(rows.ToList());

            var result = await CreateSut().Handle(
                new GetItemSpecificationValueBySpecificationMasterQuery(10),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess_With_Empty_List()
        {
            _mockQueryRepo
                .Setup(r => r.GetBySpecificationMasterIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ItemSpecificationValueDto>)new List<ItemSpecificationValueDto>());
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationValueDto>>(It.IsAny<IReadOnlyList<ItemSpecificationValueDto>>()))
                .Returns(new List<ItemSpecificationValueDto>());

            var result = await CreateSut().Handle(
                new GetItemSpecificationValueBySpecificationMasterQuery(99),
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockQueryRepo
                .Setup(r => r.GetBySpecificationMasterIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ItemSpecificationValueDto>)new List<ItemSpecificationValueDto>());
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationValueDto>>(It.IsAny<IReadOnlyList<ItemSpecificationValueDto>>()))
                .Returns(new List<ItemSpecificationValueDto>());

            await CreateSut().Handle(
                new GetItemSpecificationValueBySpecificationMasterQuery(7),
                CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(
                It.Is<AuditLogsDomainEvent>(e => e.Module == "ItemSpecificationValue"),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
