using QCManagement.Application.Common.Interfaces.IQualitySpecification;
using QCManagement.Application.QualitySpecification.Dto;
using QCManagement.Application.QualitySpecification.Queries.GetAllQualitySpecification;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QualitySpecification.Queries
{
    public class GetAllQualitySpecificationQueryHandlerTests
    {
        private readonly Mock<IQualitySpecificationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GetAllQualitySpecificationQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        private void SetupMediator() =>
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var list = new List<QualitySpecificationListDto> { QualitySpecificationBuilders.ValidListDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null, null, null, null, null))
                .ReturnsAsync((list, 1));
            SetupMediator();

            var result = await CreateSut().Handle(
                new GetAllQualitySpecificationQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var list = new List<QualitySpecificationListDto> { QualitySpecificationBuilders.ValidListDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(2, 5, "search", null, null, null, null, null))
                .ReturnsAsync((list, 11));
            SetupMediator();

            var result = await CreateSut().Handle(
                new GetAllQualitySpecificationQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null, null, null, null, null))
                .ReturnsAsync((new List<QualitySpecificationListDto>(), 0));
            SetupMediator();

            var result = await CreateSut().Handle(
                new GetAllQualitySpecificationQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PassesAllFilters()
        {
            var list = new List<QualitySpecificationListDto> { QualitySpecificationBuilders.ValidListDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, 1, 17, 5, null, true))
                .ReturnsAsync((list, 1));
            SetupMediator();

            await CreateSut().Handle(
                new GetAllQualitySpecificationQuery
                {
                    PageNumber = 1, PageSize = 10,
                    QualityTemplateId = 1,
                    ApplicableLevelId = 17,
                    ItemCategoryId = 5,
                    IsActive = true
                },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllAsync(1, 10, null, 1, 17, 5, null, true), Times.Once);
        }
    }
}
