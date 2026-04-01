using Contracts.Common;
using InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule;
using InventoryManagement.Application.Item.PutAway.Commands.DeletePutAwayRule;
using InventoryManagement.Application.Item.PutAway.Commands.UpdatePutAwayRule;
using InventoryManagement.Application.Item.PutAway.Queries.GetAllPutAwayRule;
using InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRuleById;
using InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRuleItemId;
using InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRules;
using InventoryManagement.Presentation.Controllers.Item;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class PutAwayRuleControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private PutAwayRuleController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAllAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPutAwayRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<PutAwayRuleListDto>>
                {
                    IsSuccess = true,
                    Data = new List<PutAwayRuleListDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAllAsync_CallsMediatorOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPutAwayRulesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<PutAwayRuleListDto>>
                {
                    IsSuccess = true,
                    Data = new List<PutAwayRuleListDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetPutAwayRulesQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPutAwayRuleByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PutAwayRuleDetailDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            var cmd = new CreatePutAwayRuleCommand(new CreatePutAwayRuleRequest());
            _mockMediator
                .Setup(m => m.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(cmd);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdatePutAwayRuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MediatR.Unit.Value);

            var result = await CreateSut().UpdateAsync(new UpdatePutAwayRuleCommand(1, new CreatePutAwayRuleRequest()));

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeletePutAwayRuleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().DeleteAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPutAwayRuleLoad_WithValidIds_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPutAwayRuleItemIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetPutAwayRuleItemIdDto> { new() });

            var result = await CreateSut().GetPutAwayRuleLoad("1,2", "3,4");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
