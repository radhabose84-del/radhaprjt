using Contracts.Common;
using Contracts.Dtos.Lookups.Purchase;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.VendorRatingGrade.Commands.CreateVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Commands.DeleteVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Commands.UpdateVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Dto;
using PurchaseManagement.Application.VendorRatingGrade.Queries.GetAllVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Queries.GetVendorRatingGradeAutoComplete;
using PurchaseManagement.Application.VendorRatingGrade.Queries.GetVendorRatingGradeById;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class VendorRatingGradeControllerTests
    {
        private readonly Mock<ISender> _mockSender = new(MockBehavior.Loose);

        private VendorRatingGradeController CreateSut() => new(_mockSender.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockSender.Setup(m => m.Send(It.IsAny<GetAllVendorRatingGradeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<VendorRatingGradeDto>> { IsSuccess = true, Data = new List<VendorRatingGradeDto>(), TotalCount = 0, PageNumber = 1, PageSize = 10 });
            var result = await CreateSut().GetAllVendorRatingGradeAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockSender.Setup(m => m.Send(It.IsAny<GetVendorRatingGradeByIdQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(VendorRatingGradeBuilders.ValidDto());
            var result = await CreateSut().GetVendorRatingGradeByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockSender.Setup(m => m.Send(It.IsAny<GetVendorRatingGradeAutoCompleteQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(VendorRatingGradeBuilders.ValidLookupList());
            var result = await CreateSut().GetVendorRatingGradeAutoCompleteAsync("Exc");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockSender.Setup(m => m.Send(It.IsAny<CreateVendorRatingGradeCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });
            var result = await CreateSut().CreateVendorRatingGrade(VendorRatingGradeBuilders.ValidCreateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockSender.Setup(m => m.Send(It.IsAny<UpdateVendorRatingGradeCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });
            var result = await CreateSut().UpdateVendorRatingGrade(VendorRatingGradeBuilders.ValidUpdateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockSender.Setup(m => m.Send(It.IsAny<DeleteVendorRatingGradeCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var result = await CreateSut().DeleteVendorRatingGrade(1);
            result.Should().BeOfType<OkObjectResult>();
        }
    }
}
