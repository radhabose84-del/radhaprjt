using AutoMapper;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationValue;
using InventoryManagement.Application.ItemSpecificationValue.Dto;
using InventoryManagement.Application.ItemSpecificationValue.Queries.GetAllItemSpecificationValue;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.ItemSpecificationValue.Queries
{
    public sealed class GetAllItemSpecificationValueQueryHandlerTests
    {
        private readonly Mock<IItemSpecificationValueQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllItemSpecificationValueQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var emptyList = new List<ItemSpecificationValueDto>();
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 15, null))
                .ReturnsAsync((emptyList, 0));
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationValueDto>>(It.IsAny<object>()))
                .Returns(emptyList);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAllItemSpecificationValueQuery { PageNumber = 1, PageSize = 15, SearchTerm = null },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsMappedDtos()
        {
            var dtoList = new List<ItemSpecificationValueDto>
            {
                ItemSpecificationValueBuilders.ValidDto(1),
                ItemSpecificationValueBuilders.ValidDto(2)
            };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 15, null))
                .ReturnsAsync((dtoList, 2));
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationValueDto>>(It.IsAny<object>()))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllItemSpecificationValueQuery { PageNumber = 1, PageSize = 15, SearchTerm = null },
                CancellationToken.None);

            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<ItemSpecificationValueDto>
            {
                ItemSpecificationValueBuilders.ValidDto(1)
            };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(2, 5, "search"))
                .ReturnsAsync((dtoList, 11));
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationValueDto>>(It.IsAny<object>()))
                .Returns(dtoList);

            var result = await CreateSut().Handle(
                new GetAllItemSpecificationValueQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }
    }
}
