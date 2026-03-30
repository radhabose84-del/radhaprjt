using FluentValidation;
using FluentValidation.Results;
using InventoryManagement.Application.UOM.Command.CreateUOM;
using InventoryManagement.Application.UOM.Command.DeleteUOM;
using InventoryManagement.Application.UOM.Command.UpdateUOM;
using InventoryManagement.Application.UOM.Queries.GetUOMAutoComplete;
using InventoryManagement.Application.UOM.Queries.GetUOMById;
using InventoryManagement.Application.UOM.Queries.GetUOMs;
using InventoryManagement.Application.UOM.Queries.GetUOMTypeAutoComplete;
using InventoryManagement.Presentation.Controllers;
using InventoryManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Contracts.Common;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class UOMControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<CreateUOMCommand>> _mockCreateValidator = new(MockBehavior.Strict);
        private readonly Mock<IValidator<UpdateUOMCommand>> _mockUpdateValidator = new(MockBehavior.Strict);

        private UOMController CreateSut() =>
            new(_mockMediator.Object, _mockCreateValidator.Object, _mockUpdateValidator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUOMQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<UOMDto>>
                {
                    IsSuccess = true,
                    Data = new List<UOMDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllUOMAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUOMQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<UOMDto>>
                {
                    IsSuccess = true,
                    Data = new List<UOMDto>()
                });

            await CreateSut().GetAllUOMAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetUOMQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUOMByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<UOMDto>
                {
                    IsSuccess = true,
                    Data = UOMBuilders.ValidDto()
                });

            var result = await CreateSut().GetByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_WithValidCommand_ReturnsOkResult()
        {
            _mockCreateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<CreateUOMCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateUOMCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<UOMDto>
                {
                    IsSuccess = true,
                    Data = UOMBuilders.ValidDto()
                });

            var result = await CreateSut().CreateAsync(UOMBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_WithValidCommand_ReturnsOkResult()
        {
            _mockUpdateValidator
                .Setup(v => v.ValidateAsync(It.IsAny<UpdateUOMCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUOMByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<UOMDto>
                {
                    IsSuccess = true,
                    Data = UOMBuilders.ValidDto()
                });

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateUOMCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<bool>
                {
                    IsSuccess = true,
                    Data = true
                });

            var result = await CreateSut().Update(UOMBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteUOMCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<UOMDto>
                {
                    IsSuccess = true,
                    Data = UOMBuilders.ValidDto()
                });

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetUOMAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<UOMAutoCompleteDto>>
                {
                    IsSuccess = true,
                    Data = new List<UOMAutoCompleteDto>()
                });

            var result = await CreateSut().GetUOM("KG");

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
