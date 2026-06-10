using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.Arrival.Commands.CreateArrival;
using PurchaseManagement.Application.Arrival.Commands.DeleteArrival;
using PurchaseManagement.Application.Arrival.Commands.UpdateArrival;
using PurchaseManagement.Application.Arrival.Dto;
using PurchaseManagement.Application.Arrival.Queries.GetAllArrival;
using PurchaseManagement.Application.Arrival.Queries.GetArrivalAutoComplete;
using PurchaseManagement.Application.Arrival.Queries.GetArrivalById;
using PurchaseManagement.Presentation.Controllers;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class ArrivalControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private ArrivalController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllArrivalQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ArrivalDto>>
                {
                    IsSuccess = true,
                    Data = new List<ArrivalDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllArrivalAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllArrivalQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<ArrivalDto>> { IsSuccess = true, Data = new List<ArrivalDto>() });

            await CreateSut().GetAllArrivalAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllArrivalQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetArrivalByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ArrivalBuilders.ValidDto());

            var result = await CreateSut().GetArrivalByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetArrivalAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ArrivalBuilders.ValidLookupList());

            var result = await CreateSut().GetArrivalAutoCompleteAsync("ARV");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateArrivalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateArrival(ArrivalBuilders.ValidCreateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateArrivalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateArrival(ArrivalBuilders.ValidUpdateCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteArrivalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteArrival(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteArrivalCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteArrival(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteArrivalCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
