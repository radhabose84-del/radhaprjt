using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.Templates.Queries.GetInspectionTemplateAutoComplete;
using InventoryManagement.Application.Item.Templates.Queries.GetTemplateAutoComplete;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;

namespace InventoryManagement.UnitTests.Application.Templates.Queries
{
    public sealed class GetTemplateAutoCompleteQueryHandlerTests
    {
        private readonly Mock<ITemplateQueryRepository> _mockQry = new(MockBehavior.Strict);

        private GetTemplateAutoCompleteQueryHandler CreateSut() =>
            new(_mockQry.Object);

        [Fact]
        public async Task Handle_ReturnsMatchingTemplates()
        {
            var items = new List<InspectionTemplate>
            {
                new() { Id = 1, TemplateName = "Quality Check" }
            };
            _mockQry.Setup(r => r.GetAutoCompleteAsync("Quality", 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<InspectionTemplate>)items);

            var result = await CreateSut().Handle(
                new GetTemplateAutoCompleteQuery { SearchPattern = "Quality", Take = 10 }, CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].TemplateName.Should().Be("Quality Check");
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyList()
        {
            _mockQry.Setup(r => r.GetAutoCompleteAsync(null, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<InspectionTemplate>)new List<InspectionTemplate>());

            var result = await CreateSut().Handle(
                new GetTemplateAutoCompleteQuery { SearchPattern = null }, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
