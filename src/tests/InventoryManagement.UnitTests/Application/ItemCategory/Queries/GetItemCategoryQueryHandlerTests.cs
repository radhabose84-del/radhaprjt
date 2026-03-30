using AutoMapper;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.Item.ItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategory;
using InventoryManagement.Domain.Events;
using MediatR;

namespace InventoryManagement.UnitTests.Application.ItemCategory.Queries
{
    public sealed class GetItemCategoryQueryHandlerTests
    {
        private readonly Mock<IItemCategoryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetItemCategoryQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dynamics = (IEnumerable<dynamic>)new List<dynamic>();
            _mockQueryRepo.Setup(r => r.GetAllItemCategoryAsync(1, 10, null))
                .ReturnsAsync((dynamics, 0));
            _mockMapper.Setup(m => m.Map<List<ItemCategoryDto>>(It.IsAny<IEnumerable<dynamic>>()))
                .Returns(new List<ItemCategoryDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetItemCategoryQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_WithRecords_ReturnsMappedDtos()
        {
            var dynamics = (IEnumerable<dynamic>)new List<dynamic> { new { Id = 1 } };
            var dtoList = new List<ItemCategoryDto> { new ItemCategoryDto { Id = 1, ItemCategoryName = "Cat" } };
            _mockQueryRepo.Setup(r => r.GetAllItemCategoryAsync(1, 10, null))
                .ReturnsAsync((dynamics, 1));
            _mockMapper.Setup(m => m.Map<List<ItemCategoryDto>>(It.IsAny<IEnumerable<dynamic>>()))
                .Returns(dtoList);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetItemCategoryQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.Data.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dynamics = (IEnumerable<dynamic>)new List<dynamic>();
            _mockQueryRepo.Setup(r => r.GetAllItemCategoryAsync(2, 5, "search"))
                .ReturnsAsync((dynamics, 20));
            _mockMapper.Setup(m => m.Map<List<ItemCategoryDto>>(It.IsAny<IEnumerable<dynamic>>()))
                .Returns(new List<ItemCategoryDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetItemCategoryQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(20);
        }
    }
}
