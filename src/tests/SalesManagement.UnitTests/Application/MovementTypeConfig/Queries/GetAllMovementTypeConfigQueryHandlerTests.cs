using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMovementTypeConfig;
using SalesManagement.Application.MovementTypeConfig.Dto;
using SalesManagement.Application.MovementTypeConfig.Queries.GetAllMovementTypeConfig;

namespace SalesManagement.UnitTests.Application.MovementTypeConfig.Queries
{
    public class GetAllMovementTypeConfigQueryHandlerTests
    {
        private readonly Mock<IMovementTypeConfigQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAllMovementTypeConfigQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<MovementTypeConfigDto>>(It.IsAny<object>()))
                .Returns<object>(o => o as List<MovementTypeConfigDto> ?? new List<MovementTypeConfigDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetAllMovementTypeConfigQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
        {
            var data = new List<MovementTypeConfigDto>
            {
                new() { Id = 1, MovementCode = "M001" },
                new() { Id = 2, MovementCode = "M002" }
            };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((data, 2));

            var result = await CreateSut().Handle(
                new GetAllMovementTypeConfigQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsCorrectPaginationMetadata()
        {
            var data = new List<MovementTypeConfigDto> { new() { Id = 1 } };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "move"))
                .ReturnsAsync((data, 11));

            var result = await CreateSut().Handle(
                new GetAllMovementTypeConfigQuery { PageNumber = 2, PageSize = 5, SearchTerm = "move" },
                CancellationToken.None);

            result.TotalCount.Should().Be(11);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<MovementTypeConfigDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllMovementTypeConfigQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetAllAsync_Once_WithCorrectParameters()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 20, "test"))
                .ReturnsAsync((new List<MovementTypeConfigDto>(), 0));

            await CreateSut().Handle(
                new GetAllMovementTypeConfigQuery { PageNumber = 1, PageSize = 20, SearchTerm = "test" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 20, "test"), Times.Once);
        }
    }
}
