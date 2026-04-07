using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IFreightMaster;
using SalesManagement.Application.FreightMaster.Dto;
using SalesManagement.Application.FreightMaster.Queries.GetAllFreightMaster;

namespace SalesManagement.UnitTests.Application.FreightMaster.Queries
{
    public class GetAllFreightMasterQueryHandlerTests
    {
        private readonly Mock<IFreightMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAllFreightMasterQueryHandler CreateSut()
        {
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetAllFreightMasterQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
        {
            var data = new List<FreightMasterDto>
            {
                new() { Id = 1, FreightModeId = 1, Rate = 100m },
                new() { Id = 2, FreightModeId = 2, Rate = 200m }
            };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((data, 2));

            var result = await CreateSut().Handle(
                new GetAllFreightMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsCorrectPaginationMetadata()
        {
            var data = new List<FreightMasterDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "freight"))
                .ReturnsAsync((data, 11));

            var result = await CreateSut().Handle(
                new GetAllFreightMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "freight" },
                CancellationToken.None);

            result.TotalCount.Should().Be(11);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<FreightMasterDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllFreightMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetAllAsync_Once_WithCorrectParameters()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 20, "test"))
                .ReturnsAsync((new List<FreightMasterDto>(), 0));

            await CreateSut().Handle(
                new GetAllFreightMasterQuery { PageNumber = 1, PageSize = 20, SearchTerm = "test" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 20, "test"), Times.Once);
        }
    }
}
