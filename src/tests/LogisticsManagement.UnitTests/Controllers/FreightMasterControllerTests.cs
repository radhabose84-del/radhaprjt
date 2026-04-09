using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using LogisticsManagement.Presentation.Controllers;
using LogisticsManagement.Application.FreightMaster.Commands.CreateFreightMaster;
using LogisticsManagement.Application.FreightMaster.Commands.UpdateFreightMaster;
using LogisticsManagement.Application.FreightMaster.Commands.DeleteFreightMaster;
using LogisticsManagement.Application.FreightMaster.Queries.GetAllFreightMaster;
using LogisticsManagement.Application.FreightMaster.Queries.GetFreightMasterById;
using LogisticsManagement.Application.FreightMaster.Queries.GetFreightMasterAutoComplete;
using LogisticsManagement.Application.FreightMaster.Dto;

namespace LogisticsManagement.UnitTests.Controllers
{
    public sealed class FreightMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private FreightMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllFreightMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<FreightMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<FreightMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllFreightMasterAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllFreightMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<FreightMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<FreightMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllFreightMasterAsync(1, 10);
            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllFreightMasterQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFreightMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FreightMasterDto { Id = 1 });

            var result = await CreateSut().GetFreightMasterByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetFreightMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FreightMasterLookupDto>());

            var result = await CreateSut().GetFreightMasterAutoCompleteAsync("test");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateFreightMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            var command = new CreateFreightMasterCommand { FreightModeId = 1, RateMethodId = 2, Rate = 100, ModuleId = 1 };
            var result = await CreateSut().CreateFreightMaster(command);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateFreightMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Updated", Data = 1 });

            var command = new UpdateFreightMasterCommand { Id = 1, FreightModeId = 1, RateMethodId = 2, Rate = 200, ModuleId = 1, IsActive = 1 };
            var result = await CreateSut().UpdateFreightMaster(command);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteFreightMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteFreightMaster(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteFreightMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteFreightMaster(1);
            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteFreightMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
