using Contracts.Common;
using Contracts.Dtos.Lookups.Purchase;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.VendorEvaluationCriteria.Commands.CreateVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Commands.DeleteVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Commands.UpdateVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Dto;
using PurchaseManagement.Application.VendorEvaluationCriteria.Queries.GetAllVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Queries.GetVendorEvaluationCriteriaAutoComplete;
using PurchaseManagement.Application.VendorEvaluationCriteria.Queries.GetVendorEvaluationCriteriaById;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class VendorEvaluationCriteriaControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Loose);

        private VendorEvaluationCriteriaController CreateSut() => new(_mockSender.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAllVendorEvaluationCriteriaQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<VendorEvaluationCriteriaDto>>
                {
                    IsSuccess = true,
                    Data = new List<VendorEvaluationCriteriaDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllVendorEvaluationCriteriaAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetAllVendorEvaluationCriteriaQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<VendorEvaluationCriteriaDto>>
                {
                    IsSuccess = true,
                    Data = new List<VendorEvaluationCriteriaDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllVendorEvaluationCriteriaAsync(1, 10);

            _mockSender.Verify(
                m => m.Send(It.IsAny<GetAllVendorEvaluationCriteriaQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetVendorEvaluationCriteriaByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(VendorEvaluationCriteriaBuilders.ValidDto());

            var result = await CreateSut().GetVendorEvaluationCriteriaByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<GetVendorEvaluationCriteriaAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(VendorEvaluationCriteriaBuilders.ValidLookupList());

            var result = await CreateSut().GetVendorEvaluationCriteriaAutoCompleteAsync("Qua");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<CreateVendorEvaluationCriteriaCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateVendorEvaluationCriteria(VendorEvaluationCriteriaBuilders.ValidCreateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<UpdateVendorEvaluationCriteriaCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateVendorEvaluationCriteria(VendorEvaluationCriteriaBuilders.ValidUpdateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockSender
                .Setup(m => m.Send(It.IsAny<DeleteVendorEvaluationCriteriaCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteVendorEvaluationCriteria(1);
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
