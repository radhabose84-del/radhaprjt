using AutoMapper;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IItemSpecificationMaster;
using InventoryManagement.Application.ItemSpecificationMaster.Dto;
using InventoryManagement.Application.ItemSpecificationMaster.Queries.GetAllItemSpecificationMaster;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.ItemSpecificationMaster.Queries
{
    public sealed class GetAllItemSpecificationMasterQueryHandlerTests
    {
        private readonly Mock<IItemSpecificationMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllItemSpecificationMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var emptyList = new List<ItemSpecificationMasterDto>();
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 15, null))
                .ReturnsAsync((emptyList, 0));
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationMasterDto>>(It.IsAny<object>()))
                .Returns(emptyList);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAllItemSpecificationMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithResults_ReturnsData()
        {
            var dtos = new List<ItemSpecificationMasterDto>
            {
                ItemSpecificationMasterBuilders.ValidDto()
            };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((dtos, 1));
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationMasterDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAllItemSpecificationMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtos = new List<ItemSpecificationMasterDto>
            {
                ItemSpecificationMasterBuilders.ValidDto()
            };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(2, 5, "test"))
                .ReturnsAsync((dtos, 11));
            _mockMapper
                .Setup(m => m.Map<List<ItemSpecificationMasterDto>>(It.IsAny<object>()))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetAllItemSpecificationMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }
    }
}
