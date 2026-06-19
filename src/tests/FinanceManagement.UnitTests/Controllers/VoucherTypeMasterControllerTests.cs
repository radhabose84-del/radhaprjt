using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers;
using FinanceManagement.Application.VoucherType.Commands.CreateVoucherType;
using FinanceManagement.Application.VoucherType.Commands.UpdateVoucherType;
using FinanceManagement.Application.VoucherType.Commands.DeleteVoucherType;
using FinanceManagement.Application.VoucherType.Commands.ResetVoucherTypeSeries;
using FinanceManagement.Application.VoucherType.Queries.GetAllVoucherType;
using FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeById;
using FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeAutoComplete;
using FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeSummary;
using FinanceManagement.Application.VoucherType.Queries.GetVoucherTypeNumberSeries;
using FinanceManagement.Application.VoucherType.Dto;
using FinanceManagement.UnitTests.TestData;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class VoucherTypeMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private VoucherTypeMasterController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllVoucherTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<VoucherTypeMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<VoucherTypeMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllVoucherTypeAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllVoucherTypeQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<VoucherTypeMasterDto>> { IsSuccess = true, Data = new List<VoucherTypeMasterDto>() });

            await CreateSut().GetAllVoucherTypeAsync(1, 10);

            _mockMediator.Verify(m => m.Send(It.IsAny<GetAllVoucherTypeQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Summary_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetVoucherTypeSummaryQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new VoucherTypeSummaryDto { TotalCount = 7 });

            var result = await CreateSut().GetVoucherTypeSummaryAsync();
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task NumberSeries_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetVoucherTypeNumberSeriesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<VoucherTypeNumberSeriesDto>>
                {
                    IsSuccess = true,
                    Data = VoucherTypeBuilders.ValidNumberSeriesList()
                });

            var result = await CreateSut().GetVoucherTypeNumberSeriesAsync(3);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetVoucherTypeByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(VoucherTypeBuilders.ValidDto());

            var result = await CreateSut().GetVoucherTypeByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetVoucherTypeAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<VoucherTypeLookupDto>)VoucherTypeBuilders.ValidLookupList());

            var result = await CreateSut().GetVoucherTypeAutoCompleteAsync("JV");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateVoucherTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            var result = await CreateSut().CreateVoucherType(VoucherTypeBuilders.ValidCreateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateVoucherTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Updated", Data = 1 });

            var result = await CreateSut().UpdateVoucherType(VoucherTypeBuilders.ValidUpdateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task ResetSeries_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<ResetVoucherTypeSeriesCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Reset", Data = 1 });

            var result = await CreateSut().ResetVoucherTypeSeries(new ResetVoucherTypeSeriesCommand { VoucherTypeId = 1, FinancialYearId = 3 });
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteVoucherTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteVoucherType(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSendOnce()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteVoucherTypeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteVoucherType(1);

            _mockMediator.Verify(m => m.Send(It.IsAny<DeleteVoucherTypeCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
