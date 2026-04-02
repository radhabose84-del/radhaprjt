using Microsoft.AspNetCore.Mvc;
using FinanceManagement.Presentation.Controllers;
using FinanceManagement.Application.EWaybillHeader.Commands.CreateEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Commands.UpdateEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Commands.DeleteEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Queries.GetAllEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Queries.GetEWaybillHeaderById;
using FinanceManagement.Application.EWaybillHeader.Queries.GetEWaybillHeaderAutoComplete;
using FinanceManagement.Application.EWaybillHeader.Dto;

namespace FinanceManagement.UnitTests.Controllers
{
    public sealed class EWaybillHeaderControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private EWaybillHeaderController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllEWaybillHeaderQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<EWaybillHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<EWaybillHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllEWaybillHeaderAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllEWaybillHeaderQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<EWaybillHeaderDto>>
                {
                    IsSuccess = true,
                    Data = new List<EWaybillHeaderDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllEWaybillHeaderAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllEWaybillHeaderQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEWaybillHeaderByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EWaybillHeaderDto());

            var result = await CreateSut().GetEWaybillHeaderByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetEWaybillHeaderAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<EWaybillHeaderLookupDto>)new List<EWaybillHeaderLookupDto>());

            var result = await CreateSut().GetEWaybillHeaderAutoCompleteAsync("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEWaybillHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Created", Data = 1 });

            var result = await CreateSut().CreateEWaybillHeader(new CreateEWaybillHeaderCommand
            {
                UnitId = 1,
                EWBNumber = "EWB001"
            });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Update_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdateEWaybillHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Message = "Updated", Data = 1 });

            var result = await CreateSut().UpdateEWaybillHeader(new UpdateEWaybillHeaderCommand
            {
                Id = 1,
                IsActive = 1
            });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteEWaybillHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteEWaybillHeader(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteEWaybillHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteEWaybillHeader(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteEWaybillHeaderCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
