using MediatR;
using QCManagement.Application.Common.Interfaces.IQualityTemplate;
using QCManagement.Application.QualityTemplate.Dto;
using QCManagement.Application.QualityTemplate.Queries.GetAllQualityTemplate;
using QCManagement.Domain.Events;
using QCManagement.UnitTests.TestData;

namespace QCManagement.UnitTests.Application.QualityTemplate.Queries
{
    public class GetAllQualityTemplateQueryHandlerTests
    {
        private readonly Mock<IQualityTemplateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GetAllQualityTemplateQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMediator.Object);

        private void SetupMediator() =>
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var list = new List<QualityTemplateListDto> { QualityTemplateBuilders.ValidListDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null))
                .ReturnsAsync((list, 1));
            SetupMediator();

            var result = await CreateSut().Handle(
                new GetAllQualityTemplateQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var list = new List<QualityTemplateListDto> { QualityTemplateBuilders.ValidListDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(2, 5, "search", null))
                .ReturnsAsync((list, 11));
            SetupMediator();

            var result = await CreateSut().Handle(
                new GetAllQualityTemplateQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null, null))
                .ReturnsAsync((new List<QualityTemplateListDto>(), 0));
            SetupMediator();

            var result = await CreateSut().Handle(
                new GetAllQualityTemplateQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }
    }
}
