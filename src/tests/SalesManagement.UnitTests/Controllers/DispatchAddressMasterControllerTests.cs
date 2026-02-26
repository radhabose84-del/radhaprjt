using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.DispatchAddressMaster.Commands.CreateDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Commands.DeleteDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Commands.UpdateDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Dto;
using SalesManagement.Application.DispatchAddressMaster.Queries.GetAllDispatchAddressMaster;
using SalesManagement.Application.DispatchAddressMaster.Queries.GetDispatchAddressMasterAutoComplete;
using SalesManagement.Application.DispatchAddressMaster.Queries.GetDispatchAddressMasterById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers
{
    public sealed class DispatchAddressMasterControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DispatchAddressMasterController CreateSut() => new(_mockMediator.Object);

        // ── GetAll ────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllDispatchAddressMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<DispatchAddressMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<DispatchAddressMasterDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllDispatchAddressMasterAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllDispatchAddressMasterQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<DispatchAddressMasterDto>>
                {
                    IsSuccess = true,
                    Data = new List<DispatchAddressMasterDto>(),
                    TotalCount = 0
                });

            await CreateSut().GetAllDispatchAddressMasterAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllDispatchAddressMasterQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // ── GetById ───────────────────────────────────────────────────────────

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDispatchAddressMasterByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DispatchAddressMasterDto());

            var result = await CreateSut().GetDispatchAddressMasterByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        // ── AutoComplete ──────────────────────────────────────────────────────

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetDispatchAddressMasterAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DispatchAddressMasterLookupDto>() as IReadOnlyList<DispatchAddressMasterLookupDto>);

            var result = await CreateSut().GetDispatchAddressMasterAutoCompleteAsync("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        // ── Create ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateDispatchAddressMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateDispatchAddressMaster(new CreateDispatchAddressMasterCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        // ── Update ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateDispatchAddressMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateDispatchAddressMaster(new UpdateDispatchAddressMasterCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        // ── Delete ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDispatchAddressMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteDispatchAddressMaster(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteDispatchAddressMasterCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteDispatchAddressMaster(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteDispatchAddressMasterCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
