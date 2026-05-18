using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.GRN.GRNEntry.Commands;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.DeleteGRNDocument;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.UpdateGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.UploadGRNDocument;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPending;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGateEntryPendingPo;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetGrnPending;
using PurchaseManagement.Application.GRN.GRNEntry.Queries.GetPoPending;
using PurchaseManagement.Presentation.Controllers.GRN;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class GRNEntryControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GRNEntryController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateGRNEntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateSut().CreateAsync(new CreateGRNEntryCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UpdateAsync_ReturnsOkResult()
        {
            var result = await CreateSut().UpdateAsync(new UpdateGRNEntryCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingPoList_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetGateEntryPendingPoQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetGateEntryPendingPoDto>()));

            var result = await CreateSut().GetPendingPoList(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingPoList_WhenEmpty_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetGateEntryPendingPoQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetGateEntryPendingPoDto>()));

            var result = await CreateSut().GetPendingPoList(999);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPoPending_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPoPendingQuery>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new List<GetPoPendingDto>()));

            var result = await CreateSut().GetPoPendingAsync();

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UploadGRNDetailDocument_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UploadGrnDetailDocumentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GRNDetailImageDto { ImagePath = "TEMP_abc.png" });

            var result = await CreateSut().UploadGRNDetailDocument(new UploadGrnDetailDocumentCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task UploadGRNDetailDocument_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UploadGrnDetailDocumentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GRNDetailImageDto());

            await CreateSut().UploadGRNDetailDocument(new UploadGrnDetailDocumentCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<UploadGrnDetailDocumentCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task DeleteGRNDetailDocument_WithValidPath_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteGrnDetailDocumentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().DeleteGRNDetailDocument(
                new DeleteGrnDetailDocumentCommand { GrnDetaildocumentPath = "TEMP_abc.png" });

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteGRNDetailDocument_WithEmptyPath_ReturnsBadRequest()
        {
            var result = await CreateSut().DeleteGRNDetailDocument(
                new DeleteGrnDetailDocumentCommand { GrnDetaildocumentPath = "" });

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateGRNEntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateSut().CreateAsync(new CreateGRNEntryCommand());

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateGRNEntryCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
