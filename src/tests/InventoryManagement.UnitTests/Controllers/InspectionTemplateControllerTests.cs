using InventoryManagement.Application.Item.Templates.Commands.CreateTemplate;
using InventoryManagement.Application.Item.Templates.Commands.DeleteTemplate;
using InventoryManagement.Application.Item.Templates.Commands.UpdateTemplate;
using InventoryManagement.Application.Item.Templates.DTOs;
using InventoryManagement.Application.Item.Templates.Queries.GetInspectionTemplateAutoComplete;
using InventoryManagement.Application.Item.Templates.Queries.GetInspectionTemplateById;
using InventoryManagement.Application.Item.Templates.Queries.GetInspectionTemplates;
using InventoryManagement.Presentation.Controllers.Item;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class InspectionTemplateControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private InspectionTemplateController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllTemplatesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<TemplateListItemDto>());

            var result = await CreateSut().GetAllAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllAsync_CallsMediatorOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllTemplatesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PagedResult<TemplateListItemDto>());

            await CreateSut().GetAllAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllTemplatesQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task AutoCompleteAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTemplateAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<TemplateAutoCompleteDto>());

            var result = await CreateSut().AutoCompleteAsync(null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetInspectionTemplateByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new InspectionTemplateDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistingId_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetInspectionTemplateByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((InspectionTemplateDto?)null);

            var result = await CreateSut().GetByIdAsync(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            var cmd = new CreateTemplateCommand("Test Template", null);
            _mockMediator
                .Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(cmd);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_WithExisting_ReturnsOkResult()
        {
            var cmd = new UpdateTemplateCommand(1, "Updated", null, 1);
            _mockMediator
                .Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(cmd);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_WithNonExisting_ReturnsNotFound()
        {
            var cmd = new UpdateTemplateCommand(999, "Updated", null, 1);
            _mockMediator
                .Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateSut().UpdateAsync(cmd);

            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_WithExistingId_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteTemplateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteTemplateCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateSut().DeleteAsync(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
