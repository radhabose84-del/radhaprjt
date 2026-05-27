using Contracts.Common;
using Contracts.Dtos.Lookups.Purchase;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.DeliveryScoreRule.Commands.CreateDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Commands.DeleteDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Commands.UpdateDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Dto;
using PurchaseManagement.Application.DeliveryScoreRule.Queries.GetAllDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Queries.GetDeliveryScoreRuleAutoComplete;
using PurchaseManagement.Application.DeliveryScoreRule.Queries.GetDeliveryScoreRuleById;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class DeliveryScoreRuleControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Loose);

        private DeliveryScoreRuleController CreateSut() => new(_mockSender.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockSender.Setup(m => m.Send(It.IsAny<GetAllDeliveryScoreRuleQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<DeliveryScoreRuleDto>> { IsSuccess = true, Data = new List<DeliveryScoreRuleDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10 });
            var result = await CreateSut().GetAllDeliveryScoreRuleAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockSender.Setup(m => m.Send(It.IsAny<GetDeliveryScoreRuleByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(DeliveryScoreRuleBuilders.ValidDto());
            var result = await CreateSut().GetDeliveryScoreRuleByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockSender.Setup(m => m.Send(It.IsAny<GetDeliveryScoreRuleAutoCompleteQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(DeliveryScoreRuleBuilders.ValidLookupList());
            var result = await CreateSut().GetDeliveryScoreRuleAutoCompleteAsync("On");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockSender.Setup(m => m.Send(It.IsAny<CreateDeliveryScoreRuleCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });
            var result = await CreateSut().CreateDeliveryScoreRule(DeliveryScoreRuleBuilders.ValidCreateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockSender.Setup(m => m.Send(It.IsAny<UpdateDeliveryScoreRuleCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });
            var result = await CreateSut().UpdateDeliveryScoreRule(DeliveryScoreRuleBuilders.ValidUpdateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockSender.Setup(m => m.Send(It.IsAny<DeleteDeliveryScoreRuleCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var result = await CreateSut().DeleteDeliveryScoreRule(1);
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
