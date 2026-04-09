using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetTnCTemplateMasterAutoComplete;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.TnCTemplateMaster.Queries
{
    public sealed class TnCTemplateAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ITnCTemplateMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private TnCTemplateAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ReturnsItems()
        {
            var items = TnCTemplateMasterBuilders.ValidAutoCompleteList();
            _mockQueryRepo
                .Setup(r => r.GetTnCTemplateAutoCompleteAsync("TNC", null, null))
                .ReturnsAsync(items);

            var result = await CreateSut().Handle(
                new TnCTemplateAutoCompleteQuery { SearchPattern = "TNC" },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptySearch_ReturnsAll()
        {
            _mockQueryRepo
                .Setup(r => r.GetTnCTemplateAutoCompleteAsync(null, null, null))
                .ReturnsAsync(new List<TnCAutoCompleteDto>());

            var result = await CreateSut().Handle(
                new TnCTemplateAutoCompleteQuery(), CancellationToken.None);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_WithFilters_PassesFiltersToRepo()
        {
            _mockQueryRepo
                .Setup(r => r.GetTnCTemplateAutoCompleteAsync(null, 1, 2))
                .ReturnsAsync(new List<TnCAutoCompleteDto>());

            await CreateSut().Handle(
                new TnCTemplateAutoCompleteQuery { TemplateTypeId = 1, ApplicabilityId = 2 },
                CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetTnCTemplateAutoCompleteAsync(null, 1, 2),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CallsRepoOnce()
        {
            _mockQueryRepo
                .Setup(r => r.GetTnCTemplateAutoCompleteAsync("TNC", null, null))
                .ReturnsAsync(TnCTemplateMasterBuilders.ValidAutoCompleteList());

            await CreateSut().Handle(
                new TnCTemplateAutoCompleteQuery { SearchPattern = "TNC" },
                CancellationToken.None);

            _mockQueryRepo.Verify(
                r => r.GetTnCTemplateAutoCompleteAsync("TNC", null, null),
                Times.Once);
        }
    }
}
