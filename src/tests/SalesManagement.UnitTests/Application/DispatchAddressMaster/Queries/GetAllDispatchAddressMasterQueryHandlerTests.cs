using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Dto;
using SalesManagement.Application.DispatchAddressMaster.Queries.GetAllDispatchAddressMaster;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.DispatchAddressMaster.Queries
{
    public sealed class GetAllDispatchAddressMasterQueryHandlerTests
    {
        private readonly Mock<IDispatchAddressMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAllDispatchAddressMasterQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<DispatchAddressMasterDto>>(It.IsAny<object>()))
                .Returns<object>(o => o as List<DispatchAddressMasterDto> ?? new List<DispatchAddressMasterDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetAllDispatchAddressMasterQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        // ── Happy Path ────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<DispatchAddressMasterDto> { DispatchAddressMasterBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((dtoList, 1));

            var result = await CreateSut().Handle(
                new GetAllDispatchAddressMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<DispatchAddressMasterDto> { DispatchAddressMasterBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(2, 5, "search"))
                .ReturnsAsync((dtoList, 11));

            var result = await CreateSut().Handle(
                new GetAllDispatchAddressMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<DispatchAddressMasterDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllDispatchAddressMasterQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_WithSearchTerm_PassesTermToRepository()
        {
            var dtoList = new List<DispatchAddressMasterDto> { DispatchAddressMasterBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, "Main"))
                .ReturnsAsync((dtoList, 1));

            var result = await CreateSut().Handle(
                new GetAllDispatchAddressMasterQuery { PageNumber = 1, PageSize = 10, SearchTerm = "Main" },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }
    }
}
