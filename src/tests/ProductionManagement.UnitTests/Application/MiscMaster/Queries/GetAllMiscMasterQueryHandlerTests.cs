using ProductionManagement.Application.Common.Interfaces.IMiscMaster;
using ProductionManagement.Application.MiscMaster.Queries.GetAllMiscMaster;
using ProductionManagement.Application.MiscMaster.Dto;

namespace ProductionManagement.UnitTests.Application.MiscMaster.Queries
{
    public sealed class GetAllMiscMasterQueryHandlerTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllMiscMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<MiscMasterDto> { new() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, It.IsAny<int?>())).ReturnsAsync((dtoList, 1));
            _mockMapper.Setup(m => m.Map<List<MiscMasterDto>>(It.IsAny<object>())).Returns(dtoList);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetAllMiscMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var dtoList = new List<MiscMasterDto>();
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null, It.IsAny<int?>())).ReturnsAsync((dtoList, 0));
            _mockMapper.Setup(m => m.Map<List<MiscMasterDto>>(It.IsAny<object>())).Returns(dtoList);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetAllMiscMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<MiscMasterDto> { new() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "test", It.IsAny<int?>())).ReturnsAsync((dtoList, 11));
            _mockMapper.Setup(m => m.Map<List<MiscMasterDto>>(It.IsAny<object>())).Returns(dtoList);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetAllMiscMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.TotalCount.Should().Be(11);
        }
    }
}
