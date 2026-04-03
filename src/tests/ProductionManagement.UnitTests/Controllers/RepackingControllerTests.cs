using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Sales;
using Microsoft.AspNetCore.Mvc;
using ProductionManagement.Application.RepackingHeader.Commands.CreateRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.DeleteRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.UpdateRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Dto;
using ProductionManagement.Application.RepackingHeader.Queries.GetAllRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Queries.GetRepackingHeaderAutoComplete;
using ProductionManagement.Application.RepackingHeader.Queries.GetRepackingHeaderById;
using ProductionManagement.Presentation.Controllers;

namespace ProductionManagement.UnitTests.Controllers
{
    public sealed class RepackingHeaderControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ISalesStockLedgerService> _mockStockLedger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private RepackingHeaderController CreateSut() =>
            new(_mockMediator.Object, _mockStockLedger.Object, _mockIpService.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllRepackingHeaderQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<RepackingHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<RepackingHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllRepackingHeaderAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllRepackingHeaderQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<RepackingHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<RepackingHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllRepackingHeaderAsync(1, 10);
            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllRepackingHeaderQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRepackingHeaderByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RepackingHeaderDto { Id = 1 });

            var result = await CreateSut().GetRepackingHeaderByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetRepackingHeaderAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RepackingHeaderLookupDto>());

            var result = await CreateSut().GetRepackingHeaderAutoCompleteAsync("test");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateRepackingHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            var result = await CreateSut().CreateRepackingHeader(new CreateRepackingHeaderCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateRepackingHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Updated", Data = 1 });

            var result = await CreateSut().UpdateRepackingHeader(new UpdateRepackingHeaderCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteRepackingHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteRepackingHeader(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteRepackingHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteRepackingHeader(1);
            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteRepackingHeaderCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
