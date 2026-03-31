using Contracts.Common;
using InventoryManagement.Application.UOMConversion.Command.CreateUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.DeleteUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.UpdateUOMConversion;
using InventoryManagement.Application.UOMConversion.Queries.GetAllUOMConversion;
using InventoryManagement.Application.UOMConversion.Queries.GetUOMConversionById;
using InventoryManagement.Presentation.Controllers;
using InventoryManagement.UnitTests.TestData;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InventoryManagement.UnitTests.Controllers
{
    public sealed class UOMConversionControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Strict);

        private UOMConversionController CreateSut() =>
            new(_mockSender.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAllUOMConversionsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<UOMConversionDto>>
                {
                    IsSuccess = true,
                    Data = new List<UOMConversionDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllUOMConversionsAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsSenderOnce()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAllUOMConversionsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<UOMConversionDto>>
                {
                    IsSuccess = true,
                    Data = new List<UOMConversionDto>()
                });

            await CreateSut().GetAllUOMConversionsAsync(1, 10);

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetAllUOMConversionsQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<CreateUOMConversionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<UOMConversionDto>
                {
                    IsSuccess = true,
                    Data = UOMConversionBuilders.ValidDto()
                });

            var result = await CreateSut().CreateAsync(UOMConversionBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteUOMConversionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Delete(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<UpdateUOMConversionCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<UOMConversionDto>
                {
                    IsSuccess = true,
                    Data = UOMConversionBuilders.ValidDto()
                });

            var result = await CreateSut().Update(UOMConversionBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
