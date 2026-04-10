using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SalesManagement.Application.StoHeader.Commands.CreateStoHeader;
using SalesManagement.Application.StoHeader.Commands.DeleteStoHeader;
using SalesManagement.Application.StoHeader.Commands.UpdateStoHeader;
using SalesManagement.Application.StoHeader.Dto;
using SalesManagement.Application.StoHeader.Queries.GetAllStoHeader;
using SalesManagement.Application.StoHeader.Queries.GetPendingStoHeader;
using SalesManagement.Application.StoHeader.Queries.GetPendingStoHeaderById;
using SalesManagement.Application.StoHeader.Queries.GetStoHeaderAutoComplete;
using SalesManagement.Application.StoHeader.Queries.GetStoHeaderById;
using SalesManagement.Presentation.Controllers;

namespace SalesManagement.UnitTests.Controllers
{
    public sealed class StoHeaderControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private StoHeaderController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllStoHeaderQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<StoHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<StoHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllStoHeaderAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStoHeaderByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((StoHeaderDto?)null);

            var result = await CreateSut().GetStoHeaderByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetStoHeaderAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<StoHeaderLookupDto>() as IReadOnlyList<StoHeaderLookupDto>);

            var result = await CreateSut().GetStoHeaderAutoCompleteAsync("test");
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPending_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPendingStoHeaderQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<StoHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<StoHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetPendingStoHeaderAsync(1, 10);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPendingStoHeaderByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PendingStoHeaderByIdDto?)null);

            var result = await CreateSut().GetPendingStoHeaderByIdAsync(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateStoHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().CreateStoHeader(new CreateStoHeaderCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateStoHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            var result = await CreateSut().UpdateStoHeader(new UpdateStoHeaderCommand());
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteStoHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteStoHeader(1);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateStoHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 1 });

            await CreateSut().CreateStoHeader(new CreateStoHeaderCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateStoHeaderCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
