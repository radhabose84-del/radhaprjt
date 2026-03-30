using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.Templates.DTOs;
using InventoryManagement.Application.Item.Templates.Queries.GetInspectionTemplateById;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Templates;

namespace InventoryManagement.UnitTests.Application.Templates.Queries
{
    public sealed class GetTemplateByIdQueryHandlerTests
    {
        private readonly Mock<ITemplateQueryRepository> _mockQry = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private GetInspectionTemplateByIdQueryHandler CreateSut() =>
            new(_mockQry.Object, _mockMapper.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var entity = new InspectionTemplate { Id = 1, TemplateName = "T1" };
            var dto = new InspectionTemplateDto { Id = 1, TemplateName = "T1" };

            _mockQry.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(entity);
            _mockMapper.Setup(m => m.Map<InspectionTemplateDto>(entity)).Returns(dto);

            var result = await CreateSut().Handle(
                new GetInspectionTemplateByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsNull()
        {
            _mockQry.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((InspectionTemplate?)null);

            var result = await CreateSut().Handle(
                new GetInspectionTemplateByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
