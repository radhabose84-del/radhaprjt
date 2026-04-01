using ProductionManagement.Application.Common.Interfaces.ICountMaster;
using ProductionManagement.Application.CountMaster.Queries.GetAllCountMaster;
using ProductionManagement.Application.CountMaster.Dto;

namespace ProductionManagement.UnitTests.Application.CountMaster.Queries
{
    public sealed class GetAllCountMasterQueryHandlerTests
    {
        private readonly Mock<ICountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllCountMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<CountMasterDto> { new() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 1));
            _mockMapper.Setup(m => m.Map<List<CountMasterDto>>(It.IsAny<object>())).Returns(dtoList);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetAllCountMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var dtoList = new List<CountMasterDto>();
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 0));
            _mockMapper.Setup(m => m.Map<List<CountMasterDto>>(It.IsAny<object>())).Returns(dtoList);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetAllCountMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<CountMasterDto> { new() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "test")).ReturnsAsync((dtoList, 11));
            _mockMapper.Setup(m => m.Map<List<CountMasterDto>>(It.IsAny<object>())).Returns(dtoList);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetAllCountMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }
    }
}
