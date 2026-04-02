using ProductionManagement.Application.Common.Interfaces.IQualityMaster;
using ProductionManagement.Application.QualityMaster.Queries.GetAllQualityMaster;
using ProductionManagement.Application.QualityMaster.Dto;

namespace ProductionManagement.UnitTests.Application.QualityMaster.Queries
{
    public sealed class GetAllQualityMasterQueryHandlerTests
    {
        private readonly Mock<IQualityMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllQualityMasterQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<QualityMasterDto> { new() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 1));
            _mockMapper.Setup(m => m.Map<List<QualityMasterDto>>(It.IsAny<object>())).Returns(dtoList);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetAllQualityMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var dtoList = new List<QualityMasterDto>();
            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 0));
            _mockMapper.Setup(m => m.Map<List<QualityMasterDto>>(It.IsAny<object>())).Returns(dtoList);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetAllQualityMasterQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<QualityMasterDto> { new() };
            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "test")).ReturnsAsync((dtoList, 11));
            _mockMapper.Setup(m => m.Map<List<QualityMasterDto>>(It.IsAny<object>())).Returns(dtoList);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GetAllQualityMasterQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" }, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }
    }
}
