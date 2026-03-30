using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.TnCTemplateMaster.Command.CreateTnCTemplateMasterCommand;
using PurchaseManagement.Application.TnCTemplateMaster.Command.DeleteTnCTemplateMasterCommand;
using PurchaseManagement.Application.TnCTemplateMaster.Command.UpdateTnCTemplateMasterCommand;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetAllTnCTemplateMaster;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetTnCTemplateMasterAutoComplete;
using PurchaseManagement.Application.TnCTemplateMaster.Queries.GetTnCTemplateMasterById;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class TnCTemplateMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private TnCTemplateMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllTncTemplateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<TncTemplateMasterDto>>
                {
                    IsSuccess = true, Data = new List<TncTemplateMasterDto>(),
                    TotalCount = 0, PageNumber = 1, PageSize = 10
                });

            var result = await CreateSut().GetAllTnCTemplateMasterAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllTncTemplateQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<TncTemplateMasterDto>>
                {
                    IsSuccess = true, Data = new List<TncTemplateMasterDto>(),
                    TotalCount = 0, PageNumber = 1, PageSize = 10
                });

            await CreateSut().GetAllTnCTemplateMasterAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllTncTemplateQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetTncTemplateByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TnCTemplateMasterBuilders.ValidDto());

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<TnCTemplateAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(TnCTemplateMasterBuilders.ValidAutoCompleteList());

            var result = await CreateSut().GetPaymentTermMaster(null, null, null);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsCreatedResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateTnCTemplateMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(TnCTemplateMasterBuilders.ValidCreateCommand());

            result.Should().BeAssignableTo<IActionResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateTnCTemplateMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().UpdateAsync(TnCTemplateMasterBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteTnCTemplateMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteTnCTemplateMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Delete(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteTnCTemplateMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
