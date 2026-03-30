using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.Templates.DTOs;
using InventoryManagement.Application.Item.Templates.Queries.GetInspectionTemplates;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;

namespace InventoryManagement.UnitTests.Application.Templates.Queries
{
    public sealed class GetAllTemplatesQueryHandlerTests
    {
        private readonly Mock<ITemplateQueryRepository> _mockQry = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetAllTemplatesQueryHandler CreateSut() =>
            new(_mockQry.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ReturnsPagedResult()
        {
            var items = new List<InspectionTemplate> { new() { Id = 1, TemplateName = "T1" } };
            _mockQry.Setup(r => r.GetAllAsync(null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<InspectionTemplate>)items, 1));
            _mockMapper.Setup(m => m.Map<TemplateListItemDto>(It.IsAny<InspectionTemplate>()))
                .Returns(new TemplateListItemDto { Id = 1, TemplateName = "T1" });

            var result = await CreateSut().Handle(
                new GetAllTemplatesQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyPagedResult()
        {
            _mockQry.Setup(r => r.GetAllAsync(null, 1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(((IReadOnlyList<InspectionTemplate>)new List<InspectionTemplate>(), 0));

            var result = await CreateSut().Handle(
                new GetAllTemplatesQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }
    }
}
