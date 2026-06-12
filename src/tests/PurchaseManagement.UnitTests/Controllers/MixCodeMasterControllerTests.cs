using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.MixCodeMaster.Commands.CreateMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Commands.UpdateMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Dto;
using PurchaseManagement.Application.MixCodeMaster.Queries.GetAllMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Queries.GetMixCodeMasterAutoComplete;
using PurchaseManagement.Application.MixCodeMaster.Queries.GetMixCodeMasterById;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class MixCodeMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private MixCodeMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllMixCodeMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<MixCodeMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<MixCodeMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllMixCodeMasterAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllMixCodeMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<MixCodeMasterDto>> { IsSuccess = true, Data = new List<MixCodeMasterDto>() });

            await CreateSut().GetAllMixCodeMasterAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllMixCodeMasterQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMixCodeMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MixCodeMasterBuilders.ValidDto());

            var result = await CreateSut().GetMixCodeMasterByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetMixCodeMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<MixCodeMasterLookupDto>)new List<MixCodeMasterLookupDto>());

            var result = await CreateSut().GetMixCodeMasterAutoCompleteAsync("MIX");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateMixCodeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1, Message = "ok" });

            var result = await CreateSut().CreateMixCodeMaster(MixCodeMasterBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateMixCodeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1, Message = "ok" });

            var result = await CreateSut().UpdateMixCodeMaster(MixCodeMasterBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<PurchaseManagement.Application.MixCodeMaster.Commands.DeleteMixCodeMaster.DeleteMixCodeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteMixCodeMaster(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<PurchaseManagement.Application.MixCodeMaster.Commands.DeleteMixCodeMaster.DeleteMixCodeMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteMixCodeMaster(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<PurchaseManagement.Application.MixCodeMaster.Commands.DeleteMixCodeMaster.DeleteMixCodeMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
