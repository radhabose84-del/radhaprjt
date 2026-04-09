using MediatR;
using Microsoft.AspNetCore.Mvc;
using PurchaseManagement.Application.GRN.GateEntry.Commands.CreateGateEntry;
using PurchaseManagement.Application.GRN.GateEntry.Commands.DeleteGateEntryDocument;
using PurchaseManagement.Application.GRN.GateEntry.Commands.UploadGateEntryDocument;
using PurchaseManagement.Application.GRN.GateEntry.Queries.GetGateEntriesApprovedPo;
using PurchaseManagement.Presentation.Controllers;

namespace PurchaseManagement.UnitTests.Controllers
{
    public sealed class GateEntryControllerTests
    {
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GateEntryController CreateSut() => new(_mockMediator.Object);

        [Fact]
        public async Task CreateAsync_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateGateEntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new CreateGateEntryCommand
            {
                GateEntryDetails = new CreateGateEntryDto
                {
                    PartyId = 1,
                    UnitId = 1,
                    VehicleNumber = "KA01AB1234",
                    ReceivingTypeId = 1
                }
            };

            var result = await CreateSut().CreateAsync(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CreateAsync_CallsMediatorSend_Once()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateGateEntryCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var command = new CreateGateEntryCommand
            {
                GateEntryDetails = new CreateGateEntryDto
                {
                    PartyId = 1,
                    UnitId = 1,
                    VehicleNumber = "KA01AB1234",
                    ReceivingTypeId = 1
                }
            };

            await CreateSut().CreateAsync(command);

            _mockMediator.Verify(
                m => m.Send(It.IsAny<CreateGateEntryCommand>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UploadDocument_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<UploadGateEntryDocumentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GateEntryDocumentDto { ImagePath = "test.jpg" });

            var result = await CreateSut().UploadDocument(new UploadGateEntryDocumentCommand());

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteDocument_WithValidPath_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeleteGateEntryDocumentCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = new DeleteGateEntryDocumentCommand { GateEntrydocumentPath = "test.jpg" };

            var result = await CreateSut().DeleteDocument(command);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task DeleteDocument_WithNullPath_ReturnsBadRequest()
        {
            var command = new DeleteGateEntryDocumentCommand { GateEntrydocumentPath = null };

            var result = await CreateSut().DeleteDocument(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task DeleteDocument_WithEmptyPath_ReturnsBadRequest()
        {
            var command = new DeleteGateEntryDocumentCommand { GateEntrydocumentPath = "  " };

            var result = await CreateSut().DeleteDocument(command);

            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task GetPendingPoList_WhenFound_ReturnsOkResult()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetGateEntriesApprovedPoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<GetGateEntriesApprovedPoDto>
                {
                    new GetGateEntriesApprovedPoDto { PoId = 1, PONumber = "PO001" }
                });

            var result = await CreateSut().GetPendingPoList(1);

            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task GetPendingPoList_WhenNull_ReturnsNotFound()
        {
            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetGateEntriesApprovedPoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<GetGateEntriesApprovedPoDto>?)null);

            var result = await CreateSut().GetPendingPoList(999);

            result.Should().BeOfType<NotFoundObjectResult>();
        }
    }
}
