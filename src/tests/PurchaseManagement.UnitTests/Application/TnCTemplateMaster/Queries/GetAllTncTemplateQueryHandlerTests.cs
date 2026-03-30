using AutoMapper;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.TnCTemplateMaster.Queries
{
    public sealed class GetAllTncTemplateQueryHandlerTests
    {
        private readonly Mock<ITnCTemplateMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllTncTemplateQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var dtoList = new List<TncTemplateMasterDto> { TnCTemplateMasterBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllTncTemplateAsync(1, 10, null))
                .ReturnsAsync((dtoList, 1));

            var result = await CreateSut().Handle(
                new GetAllTncTemplateQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var dtoList = new List<TncTemplateMasterDto> { TnCTemplateMasterBuilders.ValidDto() };
            _mockQueryRepo
                .Setup(r => r.GetAllTncTemplateAsync(2, 5, "search"))
                .ReturnsAsync((dtoList, 11));

            var result = await CreateSut().Handle(
                new GetAllTncTemplateQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccessWithEmptyList()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllTncTemplateAsync(1, 10, null))
                .ReturnsAsync((new List<TncTemplateMasterDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllTncTemplateQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_CallsGetAllOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllTncTemplateAsync(1, 10, null))
                .ReturnsAsync((new List<TncTemplateMasterDto>(), 0));

            await CreateSut().Handle(
                new GetAllTncTemplateQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetAllTncTemplateAsync(1, 10, null), Times.Once);
        }
    }
}
