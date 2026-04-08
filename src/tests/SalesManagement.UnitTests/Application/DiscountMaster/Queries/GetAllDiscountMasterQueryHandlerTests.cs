using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Dto;
using SalesManagement.Application.DiscountMaster.Queries.GetAllDiscountMaster;

namespace SalesManagement.UnitTests.Application.DiscountMaster.Queries
{
    public class GetAllDiscountMasterQueryHandlerTests
    {
        private readonly Mock<IDiscountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAllDiscountMasterQueryHandler CreateSut()
        {
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetAllDiscountMasterQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        private static DiscountMasterDto ValidDto(int id = 1, string name = "Test Discount") => new()
        {
            Id = id,
            DiscountCode = "DC001",
            DiscountName = name,
            TriggerEventId = 1,
            DiscountBasisId = 2,
            ExecutionTypeId = 3,
            ValueTypeId = 4,
            SlabTypeId = 5,
            Priority = 1,
            IsActive = true,
            IsDeleted = false
        };

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
        {
            var data = new List<DiscountMasterDto> { ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((data, 1));

            var result = await CreateSut().Handle(
                new GetAllDiscountMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsCorrectData_FromRepository()
        {
            var data = new List<DiscountMasterDto>
            {
                ValidDto(id: 1, name: "Discount A"),
                ValidDto(id: 2, name: "Discount B")
            };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((data, 2));

            var result = await CreateSut().Handle(
                new GetAllDiscountMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Data!.Should().HaveCount(2);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectPaginationMetadata()
        {
            var data = new List<DiscountMasterDto> { ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "Disc"))
                .ReturnsAsync((data, 15));

            var result = await CreateSut().Handle(
                new GetAllDiscountMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "Disc" },
                CancellationToken.None);

            result.TotalCount.Should().Be(15);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<DiscountMasterDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllDiscountMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetAllAsync_Once_WithCorrectParameters()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 20, "test"))
                .ReturnsAsync((new List<DiscountMasterDto>(), 0));

            await CreateSut().Handle(
                new GetAllDiscountMasterQuery { PageNumber = 1, PageSize = 20, SearchTerm = "test" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 20, "test"), Times.Once);
        }
    }
}
