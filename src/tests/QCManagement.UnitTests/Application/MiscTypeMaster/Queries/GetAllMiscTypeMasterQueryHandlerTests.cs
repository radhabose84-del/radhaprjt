using AutoMapper;
using MediatR;
using QCManagement.Application.Common.Interfaces.IMiscTypeMaster;
using QCManagement.Application.MiscTypeMaster.Dto;
using QCManagement.Application.MiscTypeMaster.Queries.GetAllMiscTypeMaster;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.MiscTypeMaster.Queries
{
    public class GetAllMiscTypeMasterQueryHandlerTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAllMiscTypeMasterQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<MiscTypeMasterDto>>(It.IsAny<object>()))
                .Returns<object>(o => o as List<MiscTypeMasterDto> ?? new List<MiscTypeMasterDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetAllMiscTypeMasterQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
        {
            var dtoList = new List<MiscTypeMasterDto> { MiscTypeMasterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 15, null)).ReturnsAsync((dtoList, 1));

            var result = await CreateSut().Handle(
                new GetAllMiscTypeMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsCorrectData_FromRepository()
        {
            var dtoList = new List<MiscTypeMasterDto> { MiscTypeMasterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 15, null)).ReturnsAsync((dtoList, 1));

            var result = await CreateSut().Handle(
                new GetAllMiscTypeMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.Data!.Should().HaveCount(1);
            result.Data![0].MiscTypeCode.Should().Be("QP_GROUP");
        }

        [Fact]
        public async Task Handle_ReturnsCorrectPaginationMetadata()
        {
            var dtoList = new List<MiscTypeMasterDto> { MiscTypeMasterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "test")).ReturnsAsync((dtoList, 11));

            var result = await CreateSut().Handle(
                new GetAllMiscTypeMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 15, null))
                .ReturnsAsync((new List<MiscTypeMasterDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllMiscTypeMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetAllAsync_Once_WithCorrectParameters()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, "qp"))
                .ReturnsAsync((new List<MiscTypeMasterDto>(), 0));

            await CreateSut().Handle(
                new GetAllMiscTypeMasterQuery { PageNumber = 1, PageSize = 10, SearchTerm = "qp" },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 10, "qp"), Times.Once);
        }
    }
}
