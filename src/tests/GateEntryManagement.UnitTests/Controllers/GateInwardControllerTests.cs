using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using GateEntryManagement.Presentation.Controllers;
using GateEntryManagement.Application.GateInward.Commands.CreateGateInward;
using GateEntryManagement.Application.GateInward.Commands.DeleteGateInward;
using GateEntryManagement.Application.GateInward.Commands.UploadGateInwardAttachment;
using GateEntryManagement.Application.GateInward.Commands.DeleteGateInwardAttachment;
using GateEntryManagement.Application.GateInward.Queries.GetAllGateInward;
using GateEntryManagement.Application.GateInward.Queries.GetGateInwardById;
using GateEntryManagement.Application.GateInward.Queries.GetGateInwardAutoComplete;
using GateEntryManagement.Application.GateInward.Dto;

namespace GateEntryManagement.UnitTests.Controllers
{
    public sealed class GateInwardControllerTests
    {
        private readonly Mock<ISender> _mockMediator = new(MockBehavior.Strict);

        private GateInwardController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task GetAll_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllGateInwardQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GateInwardHdrDto>>
                {
                    IsSuccess = true,
                    Data = new List<GateInwardHdrDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            var result = await CreateSut().GetAllGateInwardAsync(1, 10);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetAll_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetAllGateInwardQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<List<GateInwardHdrDto>>
                {
                    IsSuccess = true,
                    Data = new List<GateInwardHdrDto>(),
                    TotalCount = 0,
                    PageNumber = 1,
                    PageSize = 10
                });

            await CreateSut().GetAllGateInwardAsync(1, 10);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<GetAllGateInwardQuery>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetGateInwardByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GateInwardHdrDto());

            var result = await CreateSut().GetGateInwardByIdAsync(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task AutoComplete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetGateInwardAutoCompleteQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GateInwardAutoCompleteDto>() as IReadOnlyList<GateInwardAutoCompleteDto>);

            var result = await CreateSut().GetGateInwardAutoCompleteAsync("test");

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Create_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateGateInwardCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int>
                {
                    IsSuccess = true,
                    Message = "Gate Inward Entry created successfully.",
                    Data = 1
                });

            var result = await CreateSut().CreateGateInward(new CreateGateInwardCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UploadAttachment_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UploadGateInwardAttachmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UploadGateInwardAttachmentResultDto("TEMP_a.pdf"));

            var result = await CreateSut().UploadAttachment(new UploadGateInwardAttachmentCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAttachment_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteGateInwardAttachmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteAttachment(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteAttachment_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteGateInwardAttachmentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteAttachment(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteGateInwardAttachmentCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteGateInwardCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteGateInward(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task Delete_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteGateInwardCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().DeleteGateInward(1);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<DeleteGateInwardCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
