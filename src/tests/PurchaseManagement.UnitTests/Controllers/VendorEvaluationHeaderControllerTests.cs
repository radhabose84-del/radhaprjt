using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.CreateVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.DeleteVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Commands.UpdateVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;
using PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetAllVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Queries.GetVendorEvaluationHeaderById;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class VendorEvaluationHeaderControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Loose);

        private VendorEvaluationHeaderController CreateSut() => new(_mockSender.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAllVendorEvaluationHeaderQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<VendorEvaluationHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<VendorEvaluationHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllVendorEvaluationHeaderAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAllVendorEvaluationHeaderQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<VendorEvaluationHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<VendorEvaluationHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllVendorEvaluationHeaderAsync(1, 10);

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetAllVendorEvaluationHeaderQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetVendorEvaluationHeaderByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(VendorEvaluationHeaderBuilders.ValidDto());

            var result = await CreateSut().GetVendorEvaluationHeaderByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<CreateVendorEvaluationHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1, Message = "Created" });

            var result = await CreateSut().CreateVendorEvaluationHeader(VendorEvaluationHeaderBuilders.ValidCreateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<UpdateVendorEvaluationHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1, Message = "Updated" });

            var result = await CreateSut().UpdateVendorEvaluationHeader(VendorEvaluationHeaderBuilders.ValidUpdateCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteVendorEvaluationHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteVendorEvaluationHeader(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteVendorEvaluationHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteVendorEvaluationHeader(1);

            _mockSender.Verify(
                m => m.Send(It.IsAny<DeleteVendorEvaluationHeaderCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
