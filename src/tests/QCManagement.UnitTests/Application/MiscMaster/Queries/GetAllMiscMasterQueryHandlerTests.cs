using AutoMapper;
using MediatR;
using QCManagement.Application.Common.Interfaces.IMiscMaster;
using QCManagement.Application.MiscMaster.Dto;
using QCManagement.Application.MiscMaster.Queries.GetAllMiscMaster;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.MiscMaster.Queries
{
    public class GetAllMiscMasterQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetAllMiscMasterQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<List<MiscMasterDto>>(It.IsAny<object>()))
                .Returns<object>(o => o as List<MiscMasterDto> ?? new List<MiscMasterDto>());
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetAllMiscMasterQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
        {
            var dtoList = new List<MiscMasterDto> { MiscMasterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 15, null, null)).ReturnsAsync((dtoList, 1));

            var result = await CreateSut().Handle(
                new GetAllMiscMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsCorrectData_FromRepository()
        {
            var dtoList = new List<MiscMasterDto> { MiscMasterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 15, null, null)).ReturnsAsync((dtoList, 1));

            var result = await CreateSut().Handle(
                new GetAllMiscMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.Data!.Should().HaveCount(1);
            result.Data![0].Code.Should().Be("PHY");
        }

        [Fact]
        public async Task Handle_ReturnsCorrectPaginationMetadata()
        {
            var dtoList = new List<MiscMasterDto> { MiscMasterBuilders.ValidDto() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "test", null)).ReturnsAsync((dtoList, 11));

            var result = await CreateSut().Handle(
                new GetAllMiscMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_FilterByMiscTypeId_PassesParameterToRepo()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, 2))
                .ReturnsAsync((new List<MiscMasterDto>(), 0));

            await CreateSut().Handle(
                new GetAllMiscMasterQuery { PageNumber = 1, PageSize = 10, MiscTypeId = 2 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 10, null, 2), Times.Once);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
        {
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 15, null, null))
                .ReturnsAsync((new List<MiscMasterDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllMiscMasterQuery { PageNumber = 1, PageSize = 15 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }
}
