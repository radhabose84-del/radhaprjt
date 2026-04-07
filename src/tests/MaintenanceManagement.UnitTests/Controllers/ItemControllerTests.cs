using MaintenanceManagement.Application.Item.ItemGroup.Queries;
using MaintenanceManagement.Application.Item.ItemMaster.Queries;
using MaintenanceManagement.Presentation.Controllers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MaintenanceManagement.UnitTests.Controllers
{
    public sealed class ItemControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<ItemController>> _mockLogger = new(MockBehavior.Loose);

        private ItemController CreateSut() => new(_mockLogger.Object, _mockMediator.Object);

        [Fact]
        public async Task GetGroupCode_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetItemGroupQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetItemGroupDto>());

            var result = await CreateSut().GetGroupCode("U001");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetGroupCode_NullParam_ReturnsBadRequest()
        {
            var result = await CreateSut().GetGroupCode(null!);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetItemMasters_ValidParams_ReturnsOkResult()
        {
            _mockMediator.Setup(m => m.Send(It.IsAny<GetItemMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetItemMasterDto>());

            var result = await CreateSut().GetItemMasters("U001", "GRP01");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetItemMasters_EmptyParams_ReturnsBadRequest()
        {
            var result = await CreateSut().GetItemMasters("", "");
            result.Should().BeOfType<BadRequestObjectResult>();
        }
    }
}
